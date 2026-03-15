function SuccessAlert(title, massage) {
    Swal.fire({
        title: title,
        icon: "success",
        text: massage,
        draggable: true,
        timer: 2000,
        timerProgressBar: true
    });
}
function ErrorAlert(title, massage) {
    Swal.fire({
        title: title,
        icon: "error",
        text: massage,
        draggable: true
    });
}
function WarningAlert(title, massage) {
    Swal.fire({
        title: title,
        icon: "warning",
        text: massage,
        draggable: true
    });
}
function InfoAlert(title, massage) {
    Swal.fire({
        title: title,
        icon: "info",
        text: massage,
        draggable: true
    });
}
function ConfirmAlert(title, massage, confirmButtonText, cancelButtonText, confirmFunction) {
    Swal.fire({
        title: title,
        icon: "warning",
        text: massage,
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: confirmButtonText,
        cancelButtonText: cancelButtonText,
        draggable: true
    }).then((result) => {
        if (result.isConfirmed) {
            confirmFunction();
        }
    });
}
function PromptAlert(title, massage, inputType, confirmButtonText, cancelButtonText, confirmFunction) {
    Swal.fire({
        title: title,
        text: massage,
        input: inputType,
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: confirmButtonText,
        cancelButtonText: cancelButtonText,
        draggable: true
    }).then((result) => {
        if (result.isConfirmed) {
            confirmFunction(result.value);
        }
    });
}