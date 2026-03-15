// configure axiosInstance
const axiosInstance = axios.create();

// Add a request interceptor
axiosInstance.interceptors.request.use(async (config) => {
    if (config.url.includes('/app/login') || config.url.includes('/app/register') || config.url.includes('Authentication/GeneratePasswordResetToken') || config.url.includes('Authentication/PasswordReset')
    ) {
        // skip modifying the request for login or register
        return config;
    }
    config.headers['Authorization'] =await getRefreshToken();  //  `Bearer ${accessToken}`;
    return config;
}, (error) => {
    return Promise.reject(error);
});
// tabulator refresh token
function GetDataforTabulator(url, config, params) {
    return new Promise(function (resolve, reject) {
        const queryParams = new URLSearchParams(params).toString();
        let finalUrl = `${url}`;
        if (queryParams !="") {
            finalUrl += `?${queryParams}`;
        }

        getData(finalUrl)
            .then(result => {
                const finalData = Array.isArray(result.data) ? result.data : result.data.data;
                resolve(finalData);
            })
            .catch(err => {
                if (err.status == 401 || err.status == 403) {
                    ErrorAlert("Sorry! You are not authorize to access this page or data");
                }
                console.error("Tabulator data load failed", err.status);
                reject(err);
            });       
    });
}
// Function to check if the token is expired
function checkTokenExpired(token) {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const currentTime = Math.floor(Date.now() / 1000);
    return payload.exp < currentTime;
}

// Function to get a new access token using the refresh token
async function getRefreshToken() {
    const accessToken = localStorage.getItem('access_token');
    const refreshToken = localStorage.getItem('refresh_token');
    if (accessToken) {
        // Check if the token is expired
        const isTokenExpired = checkTokenExpired(accessToken);
        if (isTokenExpired) {
            try {
                const response = await axios.post('/app/refreshtoken', { token: refreshToken });
                let newaccessToken = response.data.accessToken;
                localStorage.setItem('access_token', newaccessToken);
                return `Bearer ${newaccessToken}`
            } catch (error) {
                WarningAlert("Session Expaired","You need to login again");
               // alert("Session Expaired");
                setTimeout(() => {
                window.location.href = "/app/login";

                },5000)
            }
        } else {
            return `Bearer ${accessToken}`
        }
    } else {
        WarningAlert("Session Expaired", "You need to login again");
        setTimeout(() => {
            window.location.href = "/app/login";

        }, 5000)
    }
}
// Function for GET Request
async function getData(url) {
    try {
        const response = await axiosInstance.get(url);
        //console.log('GET Response:', response.data);
        return { data: response.data.value };
    } catch (error) {
        console.log('Error in GET request:', error);
        throw error;
    }
}
// Function for GET Request
async function getDataFile(url) {
    try {
        const response = await axiosInstance.get(url, {
            responseType: 'blob',
        });
        const blob = new Blob([response.data], { type: 'application/pdf' });
        const bloburl = URL.createObjectURL(blob);
        return bloburl;
    } catch (error) {
        console.log('Error in GET request:', error);
        throw error;
    }
}
// Function for POST Request
async function postData(url, data) {
    try {
        const response = await axiosInstance.post(url, data, {
            headers: {
                'Content-Type': 'application/json'
            }
        });
        //console.log('POST Response:', response.data);
        return { data: response.data.value };
    } catch (error) {
        console.log('Error in POST request:', error);
        throw error;
    }
}
// Function for PUT Request
async function putData(url, data) {
    try {
        const response = await axiosInstance.put(url, data, {
            headers: {
                'Content-Type': 'application/json'
            }
        });
        //console.log('PUT Response:', response.data);
        return { data: response.data.value };
    } catch (error) {
        console.log('Error in PUT request:', error);
        throw error;
    }
}

// Function for DELETE Request
async function deleteData(url) {
    try {
        const response = await axiosInstance.delete(url);
        //console.log('DELETE Response:', response.data);
        return { data: response.data.value };
    } catch (error) {
        console.log('Error in DELETE request:', error);
        throw error;
    }
}
// Function for multipart form data
async function postFormData(url, data) {
    try {
        const response = await axiosInstance.post(url, data, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
        //console.log('POST Response:', response.data);
        return { data: response.data.value };
    } catch (error) {
        console.log('Error in POST request:', error);
        throw error;
    }
}
