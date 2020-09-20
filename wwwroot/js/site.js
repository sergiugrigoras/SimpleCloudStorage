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
