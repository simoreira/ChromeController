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

    /// <summary>
    /// Gesture Detector class which listens for VisualGestureBuilderFrame events from the service
    /// and updates the associated GestureResultView object with the latest results for the 'Seated' gesture
    /// </summary>
    public class GestureDetector : IDisposable
    {
        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        /// <summary> Path to the gesture database that was trained with VGB </summary>
        private readonly string gestureDatabase = @"C:\Users\Ana Cruz\Desktop\chrome_voice_controller\Gesture\gestureModality\Database\gestures.gbd";

        /// <summary> Name of the discrete gesture in the database that we want to track </summary>

        private readonly string CloseChromeGesture_2 = "closeChrome";
        private readonly string CloseTabGesture = "closeTab_Right";
        private readonly string GoBackGesture = "goBack";
        private readonly string GoForwardGesture = "goForward";
        private readonly string RefreshGesture = "refresh";
        private readonly string ScrollDownGesture = "scrollDown";
        private readonly string ScrollUpGesture = "scrollUp";
        private readonly string ZoomInGesture = "zoomIn";
        private readonly string ZoomOutGesture = "zoomOut";

        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;

        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        /// <summary>
        /// Initializes a new instance of the GestureDetector class along with the gesture frame source and reader
        /// </summary>
        /// <param name="kinectSensor">Active sensor to initialize the VisualGestureBuilderFrameSource object with</param>
        /// <param name="gestureResultView">GestureResultView object to store gesture results of a single body to</param>
        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
        {

            this.GestureResultView = gestureResultView;

            if (kinectSensor == null)
            {
                throw new ArgumentNullException("kinectSensor");
            }

            if (gestureResultView == null)
            {
                throw new ArgumentNullException("gestureResultView");
            }

            //Init lifeCycleEvents
            lce = new LifeCycleEvents("ASR", "FUSION", "gestures-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            mmic = new MmiCommunication("localhost", 8000, "User1", "GESTURES"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)
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

        /// <summary>
        /// Handles gesture detection results arriving from the sensor for the associated body tracking Id
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {

                if (frame != null)
                {
                    // get the discrete gesture results which arrived with the latest frame
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;

                    if (discreteResults != null)
                    {

                        // we only have one gesture in this source object, but you can get multiple gestures
                        foreach (Gesture gesture in this.vgbFrameSource.Gestures)
                        {
                            if (gesture.GestureType == GestureType.Discrete)
                            {
                                DiscreteGestureResult result = null;
                                discreteResults.TryGetValue(gesture, out result);

                                if (result != null)
                                {
                                    /*if((result.Confidence >=0.95) && (gesture.Name.Equals(this.CloseChromeGesture_2)))
                                    {
                                        Debug.WriteLine("CloseChrome");
                                    }*/
                                    if ((result.Confidence >= 0.3) && (gesture.Name.Equals(this.GoBackGesture)))
                                    {
                                        Console.WriteLine("GoBack");
                                        sendMessage("BACK");
                                    }
                                    else if ((result.Confidence >= 0.3) && (gesture.Name.Equals(this.GoForwardGesture)))
                                    {
                                        Console.WriteLine("GoForward");
                                        sendMessage("FORWARD");
                                    }
                                    else if ((result.Confidence >= 0.5) && (gesture.Name.Equals(this.ScrollDownGesture)))
                                    {
                                        Console.WriteLine("ScrollDown");
                                        sendMessage("SCROLL_DOWN");
                                    }
                                    else if ((result.Confidence >= 0.5) && (gesture.Name.Equals(this.ScrollUpGesture)))
                                    {                                
                                        Console.WriteLine("ScrollUp");
                                        sendMessage("SCROLL_UP");
                                    }
                                    else if ((result.Confidence >= 0.2) && (gesture.Name.Equals(this.ZoomInGesture)))
                                    {
                                        Console.WriteLine("ZoomIn");
                                        sendMessage("ZOOM_IN");
                                    }
                                    else if ((result.Confidence >= 0.2) && (gesture.Name.Equals(this.ZoomOutGesture)))
                                    {
                                        Console.WriteLine("ZoomOut");
                                        sendMessage("ZOOM_OUT");
                                    }
                                    else if ((result.Confidence >= 0.8) && (gesture.Name.Equals(this.CloseTabGesture)))
                                    {
                                        Console.WriteLine("CloseTab");
                                        sendMessage("CLOSE_TAB");
                                    }
                                    else if ((result.Confidence >= 0.5) && (gesture.Name.Equals(this.RefreshGesture)))
                                    {
                                        Console.WriteLine("Refresh");
                                        sendMessage("REFRESH");
                                    }
                                }
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
