// auth-debug.js - Một script nhỏ giúp debug vấn đề đăng nhập
document.addEventListener('DOMContentLoaded', function() {
    // Chỉ chạy trên localhost
    if (!window.location.hostname.includes('localhost') && window.location.hostname !== '127.0.0.1') {
        return;
    }
    
    // Thêm nút debug ẩn vào góc dưới phải màn hình
    const debugBtn = document.createElement('button');
    debugBtn.innerText = 'Debug Auth';
    debugBtn.style.position = 'fixed';
    debugBtn.style.bottom = '10px';
    debugBtn.style.right = '10px';
    debugBtn.style.zIndex = '9999';
    debugBtn.style.opacity = '0.5';
    debugBtn.style.fontSize = '10px';
    debugBtn.style.padding = '5px';
    debugBtn.onclick = checkAuthStatus;
    
    document.body.appendChild(debugBtn);
    
    // Tự động kiểm tra khi trang load
    setTimeout(checkAuthStatus, 500);
    
    function checkAuthStatus() {
        fetch('/diag/auth')
            .then(response => response.json())
            .then(data => {
                console.log('Authentication Status:', data);
                
                let color = data.isFullyAuthenticated ? 'green' : 
                           (data.isSignedInWithIdentity || data.hasJwtToken) ? 'orange' : 'red';
                
                debugBtn.style.backgroundColor = color;
                
                if (data.isSignedInWithIdentity) {
                    console.log('%cIdentity Auth: OK', 'color:green', `Logged in as: ${data.userName}`);
                } else {
                    console.log('%cIdentity Auth: NOT LOGGED IN', 'color:red');
                }
                
                if (data.hasJwtToken) {
                    console.log('%cJWT Auth: OK', 'color:green', 'JWT token found in cookies');
                } else {
                    console.log('%cJWT Auth: MISSING', 'color:red', 'No JWT token found');
                }
                
                if (data.roles && data.roles.length > 0) {
                    console.log('User roles:', data.roles);
                }
            })
            .catch(error => {
                console.error('Auth check error:', error);
                debugBtn.style.backgroundColor = 'gray';
            });
    }
});
