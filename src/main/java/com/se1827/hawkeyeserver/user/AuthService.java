package com.se1827.hawkeyeserver.user;

import java.io.IOException;
import java.security.GeneralSecurityException;

import com.google.api.client.googleapis.auth.oauth2.GoogleIdToken;
import com.google.api.client.googleapis.auth.oauth2.GoogleIdTokenVerifier;
import com.se1827.hawkeyeserver.security.JwtService;
import com.se1827.hawkeyeserver.security.UserPrincipal;

import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;

import lombok.RequiredArgsConstructor;

@Service
@RequiredArgsConstructor
public class AuthService {
    private final GoogleIdTokenVerifier verifier;
    private final UserRepository userRepository;
    private final JwtService jwtService;

    public String verifyIdTokenAndAddUserIfNeeded(String idToken) {
        GoogleIdToken token;
        try {
            token = verifier.verify(idToken);
        } catch (GeneralSecurityException | IOException e) {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, "Invalid ID token");
        }
        if (token == null)
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST, "Invalid ID token");
        GoogleIdToken.Payload payload = token.getPayload();
        String email = payload.getEmail();
        User user = userRepository
                .findByEmail(email)
                .orElseGet(() -> userRepository.save(User.builder()
                        .email(email)
                        .name((String) payload.get("name"))
                        .picture((String) payload.get("picture")).build()));
        return jwtService.generateToken(new UserPrincipal(user));
    }
}
