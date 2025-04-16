# Secure JWT Token Passing in Embedded Applications

This project demonstrates a secure method for passing authentication tokens (JWT) from a host web application to an embedded web application using an iframe. The solution ensures that sensitive information like JWT tokens is not exposed, maintaining security while providing a seamless user experience.

## Overview

The requirement was to load an embedded app inside a host app using an iframe. The host app is responsible for authenticating users and obtaining a JWT token. This token is then used to authenticate the user in the embedded app without requiring them to log in again.

## Problem Statement

The challenge was to securely pass the JWT token from the host app to the embedded app without exposing it in the URL or client-side code. Our main concern was about the security implications of exposing the JWT.

## Solution

### Proxy API Method

The Proxy API method was implemented to address the security concerns. This method involves using an intermediary API to handle the communication between the host app and the embedded app.

#### How It Works

1. **User Authentication**: 
   - User logs into the Host App.
   - Host App authenticates the user and generates a JWT.
   - Host App saves the JWT securely.

2. **Loading the Embedded App**:
   - User visits a specific page in the Host App to access the Embedded App.
   - Host App calls the Proxy API within the iframe.

3. **Proxy API Processing**:
   - Proxy API retrieves the JWT.
   - Includes the JWT in the Authorization Header.
   - Makes an HTTP call to load the content of the Embedded App.

4. **Embedded App Processing**:
   - Embedded App reads the JWT token and extracts claims.
   - Prepares the web page and returns the HTML content to the Host App.

5. **Content Delivery**:
   - Host App updates relative URLs to absolute URLs.
   - Loads the Embedded App's HTML content in the iframe and displays the web page.

### Alternative Approaches

While the Proxy API method was chosen for its security benefits, other methods were considered:

- **Query Parameter**: Passing the JWT as an encrypted query parameter. This method requires encryption and decryption logic on both the host and embedded app sides.
- **Direct Header Passing**: Directly passing the JWT in the Authorization Header, which was deemed insecure for this use case.

## Conclusion

The Proxy API method provides a secure way to pass sensitive information like JWT tokens between a host app and an embedded app. By processing the token server-side and modifying the content before delivery, we can maintain security and user experience.

## Future Work

- Explore additional security measures, such as token expiration and refresh mechanisms.
- Explore session handling.
- Implement logging and monitoring for the Proxy API to track usage and detect potential security issues.

