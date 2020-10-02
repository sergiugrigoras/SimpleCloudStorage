﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.


$('#new-note-form').hide();
$('#update-note-form').hide();

$('#cancel-new-note').on('click', function () {
    $('#new-note-form').hide();
    $('#create-new-note').show();
});

$('#create-new-note').on('click', function () {
    $('#new-note-form').show();
    $('#new-note-title').focus();
    $(this).hide();
});

$('#cancel-update-note').on('click', function () {
    $('#new-note-form').hide();
    $('#create-new-note').show();
    $('#update-note-form').hide();
});
function editNote(id) {
    $('#new-note-form').hide();
    $('#create-new-note').hide();
    $('#update-note-form').show();
    var newTitle = $('#note-title-' + id).html();
    var newBody = $('#note-body-' + id).html();
    $('#update-note-id').val(id);
    $('#update-note-title').val(newTitle);
    $('#update-note-body').val(newBody);
    window.scrollTo(0, 0);
    $('#update-note-title').focus();

}

$('#confirm-delete').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget)
    var fsoname = button.data('fsoname')
    var fsoid = button.data('fsoid')
    var fsotype = button.data('fsotype')
    var modal = $(this)
    modal.find('#confirm-text').text('Are you sure want to delete ' + fsotype + ' ' + fsoname)
    modal.find('#fso-id').val(fsoid)
});

$('#confirm-delete-note').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget)
    var noteId = button.data('noteid')
    var noteTitle = button.data('notetitle')
    var modal = $(this)
    modal.find('#confirm-text').text('Are you sure want to delete note ' + noteTitle)
    modal.find('#delete-note-id').val(noteId)
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

$('.fso-folder').dblclick(function () {
    window.location.replace(window.location.origin + '/HomePage/' + $(this).data('id'));
});

function redirectToPage(page, id) {
    window.location.replace(window.location.origin + '/'+page+'/' + id);
}

function showalert(message, alerttype) {

    $('#alerts').append('<div id="alertdiv" class="alert ' + alerttype + '"><a class="close" data-dismiss="alert">×</a><span>' + message + '</span></div>')

    setTimeout(function () { // this will automatically close the alert and remove this if the users doesnt close it in 5 secs

        $("#alertdiv").remove();

    }, 3000);
}


var diskSize = $('#user-data').data('disksize');
var totalBytes = $('#user-data').data('totalbytes');
var used = Math.round((totalBytes * 100) / diskSize);

if (used < 75) {
    $('#disk').addClass('bg-success');
} else if (used < 90) {
    $('#disk').addClass('bg-warning');
} else {
    $('#disk').addClass('bg-danger');
}

$('#disk').attr('aria-valuenow', used).css('width', used + '%');
$('#disk-usage').html(used + '%');


function readableBytes(bytes) {
    if (bytes == 0) return '0B';
    var i = Math.floor(Math.log(bytes) / Math.log(1024)),
        sizes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

    return (bytes / Math.pow(1024, i)).toFixed(2) * 1 + ' ' + sizes[i];
}


document.querySelectorAll(".file-size").forEach(function (fs) {
    fs.innerHTML = readableBytes(fs.innerHTML);
});


var fileList = document.querySelectorAll(".file-name");
var fileListArray = [];
fileList.forEach(function (fl) {
    fileListArray.push(fl.innerHTML);
});

var maxFileSize = 52428800;
var uploadField = document.getElementById("file");
uploadField.onchange = function () {
    if (this.files[0].size > maxFileSize) {
        showalert('File is too big <b>' + readableBytes(maxFileSize) +' </b>Max', 'alert-danger')
        this.value = "";
    };
    if (fileListArray.indexOf(this.files[0].name) >= 0) {
        /*fileListArray.push(this.files[0].name);*/
        showalert('File <b>' + this.files[0].name +'</b> already exists', 'alert-danger')
        this.value = "";
    };
    if ((this.files[0].size + totalBytes) > diskSize) {
        showalert('Not enough space, disk full', 'alert-danger')
    }
};

var folderList = document.querySelectorAll(".folder-name");
var folderListArray = [];
folderList.forEach(function (fl) {
    folderListArray.push(fl.innerHTML);
});

var folderNameInput = document.getElementById("create-folder");
$("#new-folder-form").submit(function (event) {
    if (folderListArray.indexOf(folderNameInput.value) >= 0) {
        showalert('Folder <b>' + folderNameInput.value + '</b> already exists', 'alert-danger');
        event.preventDefault();
    }
    return;
});

var fsoList = document.querySelectorAll('.fso-file');
var fsoArr = []
var i = 0;
fsoList.forEach(function (fso) {
    fsoArr[i++] = fso;
/*  let id = $(fso).data('id');
    let name = $(fso).data('name');
    let date = $(fso).data('date');
    let size = $(fso).data('size');
    console.log(`${id} ${name} ${date} ${size}`);*/
})
function sortBySize(h) {
    if (h >= 0) {
        fsoArr.sort(function (a, b) {
            return $(a).data('size') - $(b).data('size');
        });
    } else {
        fsoArr.sort(function (a, b) {
            return $(b).data('size') - $(a).data('size');
        });
    }
    $('#explorer').append(fsoArr);
}

function sortByName(h) {
    if (h >= 0) {
        fsoArr.sort(function (a, b) {
            if ($(a).data('name') < $(b).data('name'))
                return -1;
            else return 1;
        });
    } else {
        fsoArr.sort(function (a, b) {
            if ($(a).data('name') > $(b).data('name'))
                return -1;
            else return 1;
        });
    }
    $('#explorer').append(fsoArr);
 } 






function copyToClipboardLink(page,id) {
    var $temp = $("<input>");
    $("body").append($temp);
    var url = window.location.origin + '/' + page + '/' + id;
    $temp.val(url).select();
    document.execCommand("copy");
    $temp.remove();
}

function copyToClipboardCurrentUrl() {
    var $temp = $("<input>");
    $("body").append($temp);
    $temp.val(window.location.href).select();
    document.execCommand("copy");
    $temp.remove();
}
