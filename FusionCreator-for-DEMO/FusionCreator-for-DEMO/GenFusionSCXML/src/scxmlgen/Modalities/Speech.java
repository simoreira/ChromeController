/* 
  *   Speech.java generated by speechmod 
 */   

package scxmlgen.Modalities; 

import scxmlgen.interfaces.IModality; 

public enum Speech implements IModality{  
        /*
	SQUARE("[shape][SQUARE]",1500),
        TRIANGLE("[shape][TRIANGLE]",1500),
        CIRCLE("[shape][CIRCLE]",1500)
        */
        REFRESH("[action][REFRESH]", 0),
        NEW_TAB("[action][NEW_TAB]", 0),
        GO_BACK("[action][GO_BACK]", 0),
        GO_FORWARD("[action][GO_FORWARD]", 0),
        A_BIT("[quantity][A_BIT]", 0),
        A_LOT("[quantity][A_LOT]", 0)
        ;

private String event; 
private int timeout;
Speech(String m, int time) {
	event=m;
	timeout=time;
}
@Override
public int getTimeOut(){
	return timeout;
}
@Override
public String getEventName(){
	return event;
}
@Override
public String getEvName(){
	return getModalityName().toLowerCase() +event.toLowerCase();
}

}