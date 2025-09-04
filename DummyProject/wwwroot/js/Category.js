var datatable;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    datatable = $('#tblData').DataTable({
        "ajax":{
        "url": "/Admin/Category/GetAll",
        "type": "GET",
        "dataType":"json"
        },
        lengthMenu: [[2, 4, 6, 8, -1], [2, 4, 6, 8, "All"]],
        "columns": [
            { "data": "name", "width": "70%" },
            {
                "data": "id", "width": "10%", "render": function (data) {
                    return `
                    <div class="text-center">
                    <a href="/Admin/Category/Upsert/${data}" class="btn btn-info">
                    <i class="fas fa-edit"></i>
                    </a>
                    <a class="btn btn-danger" onclick=Delete('/Admin/Category/Delete/${data}')>
                    <i class="fas fa-trash-alt"></i>
                    </a>
                    </div>
                    `;
                }
            }
        ]
    })
}
function Delete(url) {
    swal({
        title: "Yor are deleting a Category",
        text: "Press ok if you want to delete. Deleted data can't be restore back!!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willdelete) => {
        if (willdelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        datatable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}