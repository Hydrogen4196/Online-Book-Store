var prodTable;
$(document).ready(function () {
    loadDataTable();
    fetchSoldCounts();
})


function loadDataTable() {
    prodTable = $('#prodTable').DataTable({

        "lengthMenu": [[5, 10, 15, 20], [5, 10, 15, 20]]
    });
}
function fetchSoldCounts() {
    $.get("/Customer/Home/GetSoldCounts", function (data) {
        data.forEach(function (item) {
            $("#sold-count-" + item.productId).text(item.count);
        });
    }).fail(function () {
        console.error("Failed to fetch sold counts.");
    });
}