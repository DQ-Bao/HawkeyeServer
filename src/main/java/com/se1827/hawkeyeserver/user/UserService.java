package com.se1827.hawkeyeserver.user;

import java.util.Optional;

import com.se1827.hawkeyeserver.security.JwtService;
import com.se1827.hawkeyeserver.security.UserPrincipal;
import com.se1827.hawkeyeserver.user.dto.LoginRequest;
import com.se1827.hawkeyeserver.user.dto.RegisterRequest;

import org.springframework.http.HttpStatus;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;

import lombok.RequiredArgsConstructor;

@Service
@RequiredArgsConstructor
public class UserService {
    private final UserRepository userRepository;
    private final AuthenticationManager authenticationManager;
    private final PasswordEncoder passwordEncoder;
    private final JwtService jwtService;

    public Optional<String> login(LoginRequest req) {
        Authentication auth = authenticationManager
                .authenticate(new UsernamePasswordAuthenticationToken(req.email(), req.password()));
        UserPrincipal principal = (UserPrincipal) auth.getPrincipal();
        return Optional.ofNullable(jwtService.generateToken(principal));
    }

    public User register(RegisterRequest req) {
        userRepository.findByEmail(req.email()).ifPresent(user -> {
            throw new ResponseStatusException(HttpStatus.BAD_REQUEST,
                    String.format("User already exists with email: %s", req.email()));
        });
        String encoded = passwordEncoder.encode(req.password());
        return userRepository.save(User.builder()
                .email(req.email())
                .password(encoded)
                .build());
    }
}
