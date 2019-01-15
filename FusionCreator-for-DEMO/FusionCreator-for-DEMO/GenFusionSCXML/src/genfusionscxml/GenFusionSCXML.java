/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package genfusionscxml;

import java.io.IOException;
import scxmlgen.Fusion.FusionGenerator;
import scxmlgen.Modalities.Output;
import scxmlgen.Modalities.Speech;
import scxmlgen.Modalities.SecondMod;

/**
 *
 * @author nunof
 */
public class GenFusionSCXML {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) throws IOException {

    FusionGenerator fg = new FusionGenerator();
  
    /*
    fg.Sequence(Speech.SQUARE, SecondMod.RED, Output.SQUARE_RED);
    fg.Sequence(Speech.SQUARE, SecondMod.BLUE, Output.SQUARE_BLUE);
    fg.Sequence(Speech.SQUARE, SecondMod.YELLOW, Output.SQUARE_YELLOW);
    fg.Complementary(Speech.TRIANGLE, SecondMod.RED, Output.TRIANGLE_RED);
    fg.Complementary(Speech.TRIANGLE, SecondMod.BLUE, Output.TRIANGLE_BLUE);
    fg.Complementary(Speech.TRIANGLE, SecondMod.YELLOW, Output.TRIANGLE_YELLOW);
    fg.Complementary(Speech.CIRCLE, SecondMod.RED, Output.CIRCLE_RED);
    fg.Complementary(Speech.CIRCLE, SecondMod.BLUE, Output.CIRCLE_BLUE);
    fg.Complementary(Speech.CIRCLE, SecondMod.YELLOW, Output.CIRCLE_YELLOW);
*/
    
    //fg.Single(Speech.CIRCLE, Output.CIRCLE);  //EXAMPLE
    //fg.Redundancy(Speech.CIRCLE, SecondMod.CIRCLE, Output.CIRCLE);  //EXAMPLE
    
    fg.Complementary(SecondMod.ZOOM_IN, Speech.A_BIT, Output.ZOOM_IN_A_BIT);
    fg.Complementary(SecondMod.ZOOM_OUT, Speech.A_BIT, Output.ZOOM_OUT_A_BIT);
    fg.Complementary(SecondMod.ZOOM_IN, Speech.A_LOT, Output.ZOOM_IN_A_LOT);
    fg.Complementary(SecondMod.ZOOM_OUT, Speech.A_LOT, Output.ZOOM_IN_A_LOT);
    fg.Complementary(SecondMod.SCROLL_UP, Speech.A_BIT, Output.SCROLL_UP_A_BIT);
    fg.Complementary(SecondMod.SCROLL_DOWN, Speech.A_BIT, Output.SCROLL_DOWN_A_BIT);
    fg.Complementary(SecondMod.SCROLL_UP, Speech.A_LOT, Output.SCROLL_UP_A_LOT);
    fg.Complementary(SecondMod.SCROLL_DOWN, Speech.A_LOT, Output.SCROLL_DOWN_A_LOT);
    
    fg.Redundancy(SecondMod.GO_BACK, Speech.GO_BACK, Output.GO_BACK);
    fg.Redundancy(SecondMod.GO_FORWARD, Speech.GO_FORWARD, Output.GO_FORWARD);
    fg.Redundancy(SecondMod.REFRESH, Speech.REFRESH, Output.REFRESH);
    fg.Redundancy(SecondMod.NEW_TAB, Speech.NEW_TAB, Output.NEW_TAB);
    
    fg.Build("fusion.scxml");
        
        
    }
    
}
