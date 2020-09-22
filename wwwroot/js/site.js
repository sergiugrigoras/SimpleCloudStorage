// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
$('#confirm-delete').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget)
    var fsoname = button.data('fsoname')
    var fsoid = button.data('fsoid')
    var fsotype = button.data('fsotype')
    var modal = $(this)
    modal.find('#confirm-text').text('Are you sure want to delete ' + fsotype + ' ' + fsoname)
    modal.find('#fso-id').val(fsoid)
});


$('#confirm-share').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget)
    var fsoname = button.data('fsoname')
    var fsoid = button.data('fsoid')
    var fsotype = button.data('fsotype')
    var modal = $(this)
    modal.find('#confirm-text').text('Are you sure want to share ' + fsotype + ' ' + fsoname)
    modal.find('#fso-id').val(fsoid)
});

function readableBytes(bytes) {
    var i = Math.floor(Math.log(bytes) / Math.log(1024)),
        sizes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

    return (bytes / Math.pow(1024, i)).toFixed(2) * 1 + ' ' + sizes[i];
}

const filesSizes = document.querySelectorAll(".file-size");
filesSizes.forEach(function (fs) {
    fs.innerHTML = readableBytes(fs.innerHTML);
});


