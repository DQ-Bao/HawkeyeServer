package com.se1827.hawkeyeserver.user;

import java.util.Optional;

import jakarta.validation.Valid;

import com.se1827.hawkeyeserver.user.dto.LoginRequest;
import com.se1827.hawkeyeserver.user.dto.LoginResponse;
import com.se1827.hawkeyeserver.user.dto.RegisterRequest;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import lombok.RequiredArgsConstructor;

@RestController
@RequestMapping("/api/v1/auth")
@RequiredArgsConstructor
public class AuthController {
    private final UserService userService;
    private final Logger logger = LoggerFactory.getLogger(AuthController.class);

    @PostMapping("/login")
    public ResponseEntity<LoginResponse> login(@Valid @RequestBody LoginRequest req) {
        logger.info("Requested");
        Optional<String> maybeToken = userService.login(req);
        if (maybeToken.isEmpty()) {
            return ResponseEntity.noContent().build();
        }
        return ResponseEntity.ok(new LoginResponse(maybeToken.get()));
    }

    @PostMapping("/register")
    public ResponseEntity<User> register(@Valid @RequestBody RegisterRequest req) {
        return ResponseEntity.ok(userService.register(req));
    }
}
