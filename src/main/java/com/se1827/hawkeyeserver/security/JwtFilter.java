package com.se1827.hawkeyeserver.security;

import java.io.IOException;
import java.util.Optional;

import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import com.se1827.hawkeyeserver.user.User;
import com.se1827.hawkeyeserver.user.UserRepository;
import com.se1827.hawkeyeserver.utils.Constants;

import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.web.authentication.WebAuthenticationDetailsSource;
import org.springframework.stereotype.Component;
import org.springframework.util.AntPathMatcher;
import org.springframework.web.filter.OncePerRequestFilter;

import io.jsonwebtoken.JwtException;
import lombok.RequiredArgsConstructor;

@Component
@RequiredArgsConstructor
public class JwtFilter extends OncePerRequestFilter {
    private final JwtService jwtService;
    private final UserRepository userRepo;

    @Override
    protected void doFilterInternal(HttpServletRequest request, HttpServletResponse response, FilterChain filterChain)
            throws ServletException, IOException {
        AntPathMatcher matcher = new AntPathMatcher();
        for (String path : Constants.PUBLIC_ENDPOINTS) {
            if (matcher.match(path, request.getServletPath())) {
                filterChain.doFilter(request, response);
                return;
            }
        }
        if (SecurityContextHolder.getContext().getAuthentication() == null) {
            String authHeader = request.getHeader("Authorization");
            if (authHeader == null || !authHeader.startsWith("Bearer ")) {
                filterChain.doFilter(request, response);
                return;
            }
            String accessToken = authHeader.substring(7);
            try {
                String username = jwtService.extractSubject(accessToken);
                Optional<User> maybeUser = userRepo.findByEmail(username);
                if (maybeUser.isEmpty()) {
                    response.sendError(HttpServletResponse.SC_UNAUTHORIZED, "Invalid token");
                    return;
                }
                UserPrincipal principal = new UserPrincipal(maybeUser.get());
                if (!principal.isEnabled() || jwtService.isTokenExpired(accessToken)) {
                    response.sendError(HttpServletResponse.SC_UNAUTHORIZED, "Invalid token");
                    return;
                }

                UsernamePasswordAuthenticationToken authToken = new UsernamePasswordAuthenticationToken(principal, null,
                        principal.getAuthorities());

                authToken.setDetails(new WebAuthenticationDetailsSource().buildDetails(request));
                SecurityContextHolder.getContext().setAuthentication(authToken);
            } catch (JwtException e) {
                response.sendError(HttpServletResponse.SC_UNAUTHORIZED, "Invalid token");
                return;
            }
        }
        filterChain.doFilter(request, response);
    }
}
