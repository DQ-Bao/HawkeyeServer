package com.se1827.hawkeyeserver.user.dto;

import lombok.Getter;
import lombok.RequiredArgsConstructor;

@Getter
@RequiredArgsConstructor
public class LoginResponse {
    private final String token;
}
