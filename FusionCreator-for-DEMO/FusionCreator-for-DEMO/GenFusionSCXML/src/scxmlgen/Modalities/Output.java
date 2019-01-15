package scxmlgen.Modalities;

import scxmlgen.interfaces.IOutput;



public enum Output implements IOutput{
    
    ZOOM_IN_A_BIT("[action][ZOOM_IN][quantity][A_BIT]"),
    ZOOM_IN_A_LOT("[action][ZOOM_IN][quantity][A_LOT]"),
    ZOOM_OUT_A_BIT("[action][ZOOM_OUT][quantity][A_BIT]"),
    ZOOM_OUT_A_LOT("[action][ZOOM_OUT][quantity][A_LOT]"),
    SCROLL_UP_A_BIT("[action][SCROLL_UP][quantity][A_BIT]"),
    SCROLL_DOWN_A_BIT("[action][SCROLL_DOWN][quantity][A_BIT]"),
    SCROLL_UP_A_LOT("[action][SCROLL_UP][quantity][A_LOT]"),
    SCROLL_DOWN_A_LOT("[action][SCROLL_DOWN][quantity][A_LOT]"),
    REFRESH("[action][REFRESH]"),
    NEW_TAB("[action][NEW_TAB]"),
    GO_BACK("[action][GO_BACK]"),
    GO_FORWARD("[action][GO_FORWARD]")
    ;
    
    
    
    private String event;

    Output(String m) {
        event=m;
    }
    
    public String getEvent(){
        return this.toString();
    }

    public String getEventName(){
        return event;
    }
}
