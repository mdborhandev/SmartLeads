// baseUrl Comming from _Layout.cshtml "BaseAPI": "https://localhost:7117/api/"
//baseUrl

// this method is used to get all Users
async function getAllUsers() {
    return await getData(`${baseUrl}User`);
}

// this method is used to get User by id
async function getUserById(id) {
    return await getData(`${baseUrl}User/${id}`);
}

// this method is used to create User
async function createUser(User) {
    return await postData(`${baseUrl}User`, User);
}

// this method is used to update User
async function updateUser(User) {
    return await putData(`${baseUrl}User/${User.id}`, User);
}

// this method is used to delete User
async function deleteUser(id) {
    return await deleteData(`${baseUrl}User/${id}`);
}
async function AssignUserRoles(UserRoles) {
    return await postData(`${baseUrl}User/AssignRoles`, UserRoles);
}
async function ActiveDeactiveUser(User) {
    return await postData(`${baseUrl}User/UserActive`, User);
}

async function getUserInfo() {
    return await getData(`${baseUrl}Authentication/UserInfo`);
}

async function ChangePassword(User) {
    return await postData(`${baseUrl}Authentication/ChangePassword`, User);
}