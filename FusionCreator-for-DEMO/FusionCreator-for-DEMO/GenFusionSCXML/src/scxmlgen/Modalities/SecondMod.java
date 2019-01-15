package scxmlgen.Modalities;

import scxmlgen.interfaces.IModality;

/**
 *
 * @author nunof
 */
public enum SecondMod implements IModality{

 /*   RED("[color][RED]",1500),
    BLUE("[color][BLUE]",1500),
    YELLOW("[color][YELLOW]",1500);
    ;*/
    
   ZOOM_IN("[action][ZOOM_IN]", 0),
   ZOOM_OUT("[action][ZOOM_OUT]", 0),
   SCROLL_UP("[action][SCROLL_UP", 0),
   SCROLL_DOWN("[action][SCROLL_DOWN]", 0),
   REFRESH("[action][REFRESH]", 0),
   NEW_TAB("[action][NEW_TAB]", 0),
   GO_BACK("[action][GO_BACK]", 0),
   GO_FORWARD("[action][GO_FORWARD]", 0)
   ;
    
    private String event;
    private int timeout;


    SecondMod(String m, int time) {
        event=m;
        timeout=time;
    }

    @Override
    public int getTimeOut() {
        return timeout;
    }

    @Override
    public String getEventName() {
        //return getModalityName()+"."+event;
        return event;
    }

    @Override
    public String getEvName() {
        return getModalityName().toLowerCase()+event.toLowerCase();
    }
    
}
