//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DiscreteGestureBasics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;
    using mmisharp;
    using System.Timers;

    /// <summary>
    /// Gesture Detector class which listens for VisualGestureBuilderFrame events from the service
    /// and updates the associated GestureResultView object with the latest results for the 'Seated' gesture
    /// </summary>
    public class GestureDetector : IDisposable
    {
        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string gestureDatabase = @"Database\gestosDiscretos.gbd";

        /// <summary> Name of the discrete gesture in the database that we want to track </summary>
        private readonly string closeChromeGesture = "closeChrome";
        private readonly string closeTabGesture = "closeTab_Right";
        private readonly string goBackGesture = "goBack";
        private readonly string goForwardGesture = "goForward";
        private readonly string refreshGesture = "refresh";
        private readonly string scrollDownGesture = "scrollDown";
        private readonly string scrollUpGesture = "scrollUp";
        private readonly string zoomInGesture = "zoomIn";
        private readonly string zoomOutGesture = "zoomOut";


        private string gestureName = "";
        private float MaxConfidence = 0;
        private MainWindow main;
        private Timer MaxConfidenceTimer;
        //private bool gestureDetected = false;

        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /// <summary>
        /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        /// </summary>
        /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
        /// <param name="gestureResultView">GestureResultView object to store gesture results of a single body to</param>
        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView, MainWindow main)
        {

            this.GestureResultView = gestureResultView;
            this.main = main;
            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            if (gestureResultView == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }

            //Init lifeCycleEvents
            lce = new LifeCycleEvents("ASR", "IM", "gestures-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
            mmic.Send(lce.NewContextRequest());
            
            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            //this.vgbFrameSource.TrackingIdLost += this.Source_TrackingIdLost;

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }

            // load the gestures from the gesture database
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(this.gestureDatabase))
            {

                // we could load all available gestures in the database with a call to vgbFrameSource.AddGestures(database.AvailableGestures), 
                // but for this program, we only want to track one discrete gesture from the database, so we'll load it by name
                this.vgbFrameSource.AddGestures(database.AvailableGestures);
            }
        }

        /// <summary> Gets the GestureResultView object which stores the detector results for display in the UI </summary>
        public GestureResultView GestureResultView { get; private set; }

        /// <summary>
        /// Gets or sets the body tracking ID associated with the current detector
        /// The tracking ID can change whenever a body comes in/out of scope
        /// </summary>
        public ulong TrackingId
        {
            get
            {
                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the detector is currently paused
        /// If the body tracking ID associated with the detector is not valid, then the detector should be paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        /// <summary>
        /// Disposes all unmanaged resources for the class
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader objects
        /// </summary>
        /// <param name="disposing">True if Dispose was called directly, false if the GC handles the disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    this.vgbFrameSource.TrackingIdLost -= this.Source_TrackingIdLost;
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }

        private void ResetConfidence(Object source, ElapsedEventArgs e)
        {
            MaxConfidence = 0;
        }

        /// <summary>
        /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /// 
        int zoomInQuantity = 0;
        int zoomOutQuantity = 0;
        int scrollUpQuantity = 0;
        int scrollDownQuantity = 0;
        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            //float progress = 0;
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {

                if (frame != null)
                {
                    // get the discrete gesture results which arrived with the latest frame
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                    if( discreteResults != null)
                    {
                        foreach(Gesture gesture in vgbFrameSource.Gestures)
                        {
                            /*if(gesture.Name.Equals(this.closeTabGesture) || gesture.Name.Equals(this.goBackGesture) || gesture.Name.Equals(this.goForwardGesture) || gesture.Name.Equals(this.zoomInGesture) ||
                                gesture.Name.Equals(this.zoomOutGesture) || gesture.Name.Equals(this.scrollDownGesture) || gesture.Name.Equals(this.scrollUpGesture) || gesture.Name.Equals(this.refreshGesture) ||
                                gesture.Name.Equals(this.closeChromeGesture))
                            {*/
                            DiscreteGestureResult result = null;
                            discreteResults.TryGetValue(gesture, out result);
                            main.SetConfidence((MaxConfidence * 100).ToString("0.00"));
                            if (result != null)
                            {
                                if ((result.Confidence >= 0.5) && (gesture.Name.Equals(this.closeChromeGesture)) && (this.gestureName != this.closeChromeGesture))
                                {
                                    this.gestureName = this.closeChromeGesture;
                                    MaxConfidence = result.Confidence;
                                    main.SetGesture(gesture.Name);
                                    sendMessage("QUIT_CHROME");
                                    Console.WriteLine("CloseChrome");
                                }
                                else if ((result.Confidence >= 0.5) && (gesture.Name.Equals(this.closeTabGesture)) && (this.gestureName != this.closeTabGesture))
                                {
                                    this.gestureName = this.closeTabGesture;
                                    MaxConfidence = result.Confidence;
                                    main.SetGesture(gesture.Name);
                                    sendMessage("CLOSE_TAB");
                                    Console.WriteLine("CloseTab");
                                }
                                else if ((result.Confidence >= 0.5) && (gesture.Name.Equals(this.goBackGesture)) && (this.gestureName != this.goBackGesture))
                                {
                                    this.gestureName = this.goBackGesture;
                                    MaxConfidence = result.Confidence;
                                    main.SetGesture(gesture.Name);
                                    sendMessage("BACK");
                                    Console.WriteLine("GoBack");
                                    zoomInQuantity = 0;
                                    zoomOutQuantity = 0;
                                    scrollUpQuantity = 0;
                                    scrollDownQuantity = 0;
                                }
                                else if ((result.Confidence >= 0.2) && (gesture.Name.Equals(this.goForwardGesture)) && (this.gestureName != this.goForwardGesture))
                                {
                                    this.gestureName = this.goForwardGesture;
                                    MaxConfidence = result.Confidence;
                                    main.SetGesture(gesture.Name);
                                    sendMessage("FORWARD");
                                    Console.WriteLine("GoForward");
                                    zoomInQuantity = 0;
                                    zoomOutQuantity = 0;
                                    scrollUpQuantity = 0;
                                    scrollDownQuantity = 0;
                                }
                                else if ((result.Confidence >= 0.3) && (gesture.Name.Equals(this.zoomInGesture)))
                                {
                                    this.gestureName = this.zoomInGesture;
                                    MaxConfidence = result.Confidence;
                                    main.SetGesture(gesture.Name);
                                    if (zoomInQuantity <= 5)
                                    {
                                        sendMessage("ZOOM_IN");
                                        zoomInQuantity++;
                                    }
                                    zoomOutQuantity = 0;
                                    scrollUpQuantity = 0;
                                    scrollDownQuantity = 0;
                                    Console.WriteLine("ZoomIn");
                                }
                                else if ((result.Confidence >= 0.3) && (gesture.Name.Equals(this.zoomOutGesture)))
                                {
                                    this.gestureName = this.zoomOutGesture;
                                    MaxConfidence = result.Confidence;
                                    main.SetGesture(gesture.Name);
                                    if (zoomOutQuantity <= 5)
                                    {
                                        sendMessage("ZOOM_OUT");
                                        zoomOutQuantity++;
                                    }
                                    zoomInQuantity = 0;
                                    scrollUpQuantity = 0;
                                    scrollDownQuantity = 0;
                                    Console.WriteLine("ZoomOut");
                                }
                                else if ((result.Confidence >= 0.6) && (gesture.Name.Equals(this.refreshGesture)) && (this.gestureName != this.refreshGesture))
                                {
                                    this.gestureName = this.refreshGesture;
                                    MaxConfidence = result.Confidence;
                                    main.SetGesture(gesture.Name);
                                    sendMessage("REFRESH");
                                    Console.WriteLine("Refresh");
                                    zoomInQuantity = 0;
                                    zoomOutQuantity = 0;
                                    scrollUpQuantity = 0;
                                    scrollDownQuantity = 0;
                                }
                                else if ((result.Confidence >= 0.2) && (gesture.Name.Equals(this.scrollDownGesture)))
                                {
                                    this.gestureName = this.scrollDownGesture;
                                    MaxConfidence = result.Confidence;
                                    main.SetGesture(gesture.Name);
                                    if (scrollDownQuantity <= 5)
                                    {
                                        sendMessage("SCROLL_DOWN");
                                        scrollDownQuantity++;
                                    }
                                    zoomInQuantity = 0;
                                    zoomOutQuantity = 0;
                                    scrollUpQuantity = 0;

                                    Console.WriteLine("ScrollDown");
                                }
                                else if ((result.Confidence >= 0.4) && (gesture.Name.Equals(this.scrollUpGesture)))
                                {
                                    this.gestureName = this.scrollUpGesture;
                                    MaxConfidence = result.Confidence;
                                    main.SetGesture(gesture.Name);
                                    if (scrollUpQuantity <= 5)
                                    {
                                        sendMessage("SCROLL_UP");
                                        scrollUpQuantity++;
                                    }
                                    zoomInQuantity = 0;
                                    zoomOutQuantity = 0;
                                    scrollDownQuantity = 0;

                                    Console.WriteLine("ScrollUp");
                                }

                                if(MaxConfidenceTimer != null)
                                {
                                    MaxConfidenceTimer.Stop();
                                }

                                MaxConfidenceTimer = new Timer(2 * 1000);
                                MaxConfidenceTimer.Elapsed += ResetConfidence;
                                MaxConfidenceTimer.AutoReset = false;
                                MaxConfidenceTimer.Enabled = true;
                            }
                        }
                    }
                    
                    
                }
            }
        }

        /// <summary>
        /// Handles the TrackingIdLost event for the VisualGestureBuilderSource object
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Source_TrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            // update the GestureResultView object to show the 'Not Tracked' image in the UI
            this.GestureResultView.UpdateGestureResult(false, false, 0.0f);
        }
        
        private void sendMessage(string gesture)
        {
            string json = "{ \"recognized\": [";
            json += "\"" + gesture + "\", ";
            json = json.Substring(0, json.Length - 2);
            json += "] }";

            var exNot = lce.ExtensionNotification("", "", 1, json);
            mmic.Send(exNot); 
        }
    }
}
