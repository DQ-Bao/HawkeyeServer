package com.se1827.hawkeyeserver.user.dto;

import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotEmpty;

public record RegisterRequest(@NotEmpty @Email String email, @NotEmpty String password) {
}
