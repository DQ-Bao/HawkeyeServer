package com.se1827.hawkeyeserver.user;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/api/v1/test")
public class TestController {
    @GetMapping
    public ResponseEntity<String> test() {
        return ResponseEntity.ok("Test 2");
    }
}
