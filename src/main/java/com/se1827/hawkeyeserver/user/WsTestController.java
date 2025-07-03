package com.se1827.hawkeyeserver.user;

import org.springframework.messaging.handler.annotation.MessageMapping;
import org.springframework.messaging.handler.annotation.SendTo;
import org.springframework.stereotype.Controller;

@Controller
public class WsTestController {
    @MessageMapping("/ping")
    @SendTo("/pong")
    public String onPing() throws Exception {
        return "pong";
    }
}
