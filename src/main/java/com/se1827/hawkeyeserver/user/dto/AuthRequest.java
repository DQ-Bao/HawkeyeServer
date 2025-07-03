package com.se1827.hawkeyeserver.user.dto;

import jakarta.validation.constraints.NotEmpty;

public record AuthRequest(@NotEmpty String idToken) {
}
