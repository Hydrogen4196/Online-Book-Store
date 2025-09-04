var dataTable
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url":"/Admin/User/GetAll"
        },
        "lengthMenu": [[2, 4, 6, 8, -1], [2, 4, 6, 8, "All"]],
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "email", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "company.name", "width": "15%" },
            { "data": "role", "width": "15%" },
            {
                "data": {//object=data
                    id:"id",lockoutEnd:"lockoutEnd"//data.id data.lockoutEnd this is how we can access this object(data)
                },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        //user lock
                        return `
                        <div class="text-center">
                        <a class="btn btn-danger" onclick=LockUnlock('${data.id}')>
                        Unlock
                        </a>
                        </div>
                        `;
                    }
                    else {
                        //user unlock
                        return `
                        <div class="text-center">
                        <a class="btn btn-success" onclick=LockUnlock('${data.id}')>
                        Lock
                        </a>
                        </div>
                        `;
                    }
                }
            }
        ]
    })
}
function LockUnlock(id) {

    $.ajax({
        url: "/Admin/User/LockUnlock",
        type: "POST",
        data: JSON.stringify(id),
        contentType: "application/json",
        success:function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    })
}