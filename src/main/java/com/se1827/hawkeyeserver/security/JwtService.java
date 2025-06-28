package com.se1827.hawkeyeserver.security;

import java.time.Duration;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.function.Function;

import javax.crypto.SecretKey;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.stereotype.Service;

import io.jsonwebtoken.Claims;
import io.jsonwebtoken.JwtException;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.io.Decoders;
import io.jsonwebtoken.security.Keys;

@Service
public class JwtService {
    @Value("${spring.application.security.jwt.secret}")
    private String base64Secret;

    @Value("${spring.application.security.jwt.expiration}")
    private long jwtExpiration;

    private SecretKey getSignKey() {
        byte[] keyBytes = Decoders.BASE64.decode(base64Secret);
        return Keys.hmacShaKeyFor(keyBytes);
    }

    public String generateToken(UserDetails user) {
        return buildToken(user, new HashMap<>(), Duration.ofMillis(jwtExpiration));
    }

    private String buildToken(UserDetails user, Map<String, Object> claims, Duration expiration) {
        return Jwts.builder()
                .claims(claims)
                .subject(user.getUsername())
                .issuedAt(new Date(System.currentTimeMillis()))
                .expiration(new Date(System.currentTimeMillis() + expiration.toMillis()))
                .signWith(getSignKey())
                .compact();
    }

    private Claims extractAllClaims(String token) throws JwtException {
        return Jwts.parser()
                .verifyWith(getSignKey())
                .build()
                .parseSignedClaims(token)
                .getPayload();
    }

    public <T> T extractClaim(String token, Function<Claims, T> resolver) {
        final Claims claims = extractAllClaims(token);
        return resolver.apply(claims);
    }

    public String extractSubject(String token) {
        return extractClaim(token, Claims::getSubject);
    }

    public Date extractExpiration(String token) {
        return extractClaim(token, Claims::getExpiration);
    }

    public boolean isTokenExpired(String token) {
        try {
            Date expiration = extractExpiration(token);
            return expiration.before(new Date());
        } catch (JwtException e) {
            return true;
        }
    }

    public boolean isTokenValid(String token, UserDetails user) {
        try {
            String username = extractSubject(token);
            return username.equals(user.getUsername()) && !isTokenExpired(token);
        } catch (JwtException e) {
            return false;
        }
    }
}
