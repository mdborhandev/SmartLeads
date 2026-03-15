// baseUrl Comming from _Layout.cshtml "BaseAPI": "https://localhost:7117/api/"
//baseUrl

// this method is used to get all departments
async function getAllDepartments() {
    return await getData(`${baseUrl}department`);
}

// this method is used to get department by id
async function getDepartmentById(id) {
    return await getData(`${baseUrl}department/${id}`);
}
// this method is used to create department

async function createDepartment(department) {
    return await postData(`${baseUrl}department`, department);
}
// this method is used to update department
async function updateDepartment(department) {
    return await putData(`${baseUrl}department/${department.id}`, department);
}
// this method is used to delete department

async function deleteDepartment(id) {
    return await deleteData(`${baseUrl}department/${id}`);
}

//get department with pagination and async method
async function getDepartmentWithPagination(pageNumber, pageSize) {
    return await getData(`${baseUrl}department/pagination?pageNumber=${pageNumber}&pageSize=${pageSize}`);
}
