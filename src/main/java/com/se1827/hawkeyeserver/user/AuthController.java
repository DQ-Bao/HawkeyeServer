package com.se1827.hawkeyeserver.user;

import jakarta.validation.Valid;

import com.se1827.hawkeyeserver.user.dto.AuthRequest;
import com.se1827.hawkeyeserver.user.dto.AuthResponse;

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
    private final AuthService authService;

    @PostMapping("/google")
    public ResponseEntity<AuthResponse> googleLogin(@Valid @RequestBody AuthRequest req) {
        return ResponseEntity.ok(new AuthResponse(authService.verifyIdTokenAndAddUserIfNeeded(req.idToken())));
    }
}
