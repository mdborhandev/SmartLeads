// baseUrl Comming from _Layout.cshtml "BaseAPI": "https://localhost:7117/api/"
//baseUrl



// this method is used to create User
async function SendMassage(Data) {
    return await postData(`${baseUrl}Member/SendMassage`, Data);
}
