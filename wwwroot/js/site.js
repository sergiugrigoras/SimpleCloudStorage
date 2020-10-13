// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

//Variables
var usedBytes = 0; //Sum of all files
var totalBytes = 0;//disk size

//Functions
function showalert(message, alerttype, parentElem) {

    $(parentElem).append('<div id="alertdiv" class="alert ' + alerttype + '"><a class="close" data-dismiss="alert">×</a><span>' + message + '</span></div>')

    setTimeout(function () { // this will automatically close the alert and remove this if the users doesnt close it in 5 secs

        $("#alertdiv").remove();

    }, 3000);
}

function redirectToPage(page, id) {
    window.location.replace(window.location.origin + '/' + page + '/' + id);
}

function readableBytes(bytes) {
    if (bytes == 0) return '0B';
    var i = Math.floor(Math.log(bytes) / Math.log(1024)),
        sizes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

    return (bytes / Math.pow(1024, i)).toFixed(2) * 1 + ' ' + sizes[i];
}

function copyToClipboardLink(page, id) {
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

//Notes
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

notesBgColorArray = ['rgb(255, 191, 0)', 'rgb(255 255 130)', 'rgb(191, 255, 0)', 'rgb(101 201 255)', 'rgb(101 178 255)', 'rgb(195 134 255)'];
$('.mynote').each(function () {
    $(this).css("background-color", notesBgColorArray[Math.floor(Math.random() * notesBgColorArray.length)]);
});
//

//Delete FSO Confirm Modal
$('#confirm-delete').on('show.bs.modal', function (event) {
    var fsoCsvList = [];
    $('.fso-selected').each(function () {
        fsoCsvList.push($(this).data('id').toString());
    });
    console.log(fsoCsvList);
    var form = $('#delete-fso-form');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    var modal = $(this);
    modal.find('#confirm-text').text('Are you sure want to delete ' + fsoCsvList.length + ' file(s)');
    modal.find('#delete-fso-button').on('click', function () {
        $.ajax({
            url: '?handler=Delete',
            type: 'POST',
            data: {
                __RequestVerificationToken: token,
                fsoIdcsv: fsoCsvList.toString()
            },
            success: function () {
                modal.modal('hide');
                showalert('Successfully deleted ' + fsoCsvList.length + ' file(s)' , 'alert-success', $('#upload-file-alerts'));
                $('.fso-selected').hide();
                updateDisk();
            },
            failure: function () {
                showalert('Unable to delete', 'alert-warning', $('#upload-file-alerts'));
            }
        });
        return false;
    });
});

$('#confirm-delete').on('hidden.bs.modal', function () {
    $('#delete-fso-button').off();
});

//Delete Note Confirm Modal
$('#confirm-delete-note').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget)
    var noteId = button.data('noteid')
    var noteTitle = button.data('notetitle')
    var modal = $(this)
    modal.find('#confirm-text').text('Are you sure want to delete note ' + noteTitle)
    modal.find('#delete-note-id').val(noteId)
});

//Confirm Share Modal
$('#confirm-share').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var fsoname = $('.fso-selected').data('name');
    var fsoid = $('.fso-selected').data('id');
    var modal = $(this);
    modal.find('#confirm-text').text('Are you sure want to share file ' + fsoname);
    modal.find('#fso-id').val(fsoid);
});

//New Folder Modal
$('#create-folder-modal').on('show.bs.modal', function () {
    var folderList = document.querySelectorAll(".folder-name");
    var folderListArray = [];
    folderList.forEach(function (fl) {
        folderListArray.push(fl.innerHTML);
    });
    var folderNameInput = document.getElementById("input-create-folder");
    $("#new-folder-form").submit(function (event) {
        if (folderListArray.indexOf(folderNameInput.value) >= 0) {
            showalert('Folder <b>' + folderNameInput.value + '</b> already exists', 'alert-danger', $('#new-folder-alerts'));
            console.log('exists');
            event.preventDefault();
        }
        return;
    });
});

$('#create-folder-modal').on('hidden.bs.modal', function () {
    $("#new-folder-form").off(); //remove all events handler
});



//Upload File
$('#upload-button').on('click', function () {
    $('#input-upload-file').trigger('click');
})

var fileList = document.querySelectorAll(".file-name");
var fileListArray = [];
fileList.forEach(function (fl) {
    fileListArray.push(fl.innerHTML);
});

var maxFileSize = 52428800;
var uploadField = document.getElementById("input-upload-file");
if (uploadField != null) {
    uploadField.onchange = function () {
        if (this.files[0].size > maxFileSize) {
            showalert('File is too big <b>' + readableBytes(maxFileSize) + ' </b>Max', 'alert-danger', $('#upload-file-alerts'))
            this.value = "";
        }
        else if (fileListArray.indexOf(this.files[0].name) >= 0) {
            /*fileListArray.push(this.files[0].name);*/
            showalert('File <b>' + this.files[0].name + '</b> already exists', 'alert-danger', $('#upload-file-alerts'))
            this.value = "";
        }
        else if ((this.files[0].size + usedBytes) > totalBytes) {
            showalert('Not enough space, disk full', 'alert-danger', $('#upload-file-alerts'))
        } else {
            $('#submit-upload').trigger('click');
        }
    };
}

//

//Disk

function updateDisk() {
    $.ajax({
        type: "GET",
        url: "?handler=UserData",
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            usedBytes = response.usedBytes;
            totalBytes = response.totalBytes;
            var diskUsed = Math.round((usedBytes * 100) / totalBytes);
            $('#disk').attr('aria-valuenow', diskUsed).css('width', diskUsed + '%');
            $('#disk-usage').html(diskUsed + '%');
            $('.used-bytes').html(readableBytes(usedBytes));
            $('.total-bytes').html(readableBytes(totalBytes));

            if (diskUsed < 75) {
                $('#disk').removeClass('bg-warning bg-danger');
                $('#disk').addClass('bg-success');
            } else if (diskUsed < 90) {
                $('#disk').removeClass('bg-success bg-danger');
                $('#disk').addClass('bg-warning');
            } else {
                $('#disk').removeClass('bg-success bg-warning');
                $('#disk').addClass('bg-danger');
            }
        },
        failure: function (response) {
            alert(response);
        }
    });
}

updateDisk();


//Display Readble Bytes
document.querySelectorAll(".file-size").forEach(function (fs) {
    fs.innerHTML = readableBytes(fs.innerHTML);
});

//Download Fso
$('#download-button').on('click', function () {
    var fsoCsvList = [];
    $('.fso-selected').each(function () {
        fsoCsvList.push($(this).data('id').toString());
    });
    $('#download-fso-id-csv').val(fsoCsvList.toString());
    $('#submit-download').trigger('click');
});


//Select FSO
function hideFsoControls() {
    $('#download-form').hide();
    $('#share-button').hide();
    $('#delete-button').hide();
}

function deselectFso() {
    $('.fso').each(function () {
        $(this).removeClass("fso-selected");
    }); 
}

deselectFso();
hideFsoControls();

$(document).on('keydown', function (event) {
    if (event.key == "Escape") {
        deselectFso();
        hideFsoControls();
    }
}); 

$('.fso').on('click', function (e) {
    if (e.ctrlKey) {
        $(this).toggleClass("fso-selected");
    } else {
        deselectFso();
        $(this).addClass("fso-selected");
    }
    let selectedFiles = 0;
    let selectedFolders = 0;
    $('.fso-selected').each(function () {
        if ($(this).data('isfolder') == 'True') selectedFolders++;
        else selectedFiles++;
    });

    if (selectedFolders == 0 && selectedFiles == 0) {
        hideFsoControls();
    } else if (selectedFolders >= 1 || selectedFiles > 1) {
        $('#download-form').show();
        $('#share-button').hide();
        $('#delete-button').show();
    } else if (selectedFolders == 0 && selectedFiles == 1) {
        $('#download-form').show();
        $('#share-button').show();
        $('#delete-button').show();
    }
});



$('.fso').on('dblclick', function () {
    if ($(this).data('isfolder') == 'True') {
        redirectToPage('HomePage', $(this).data('id'));
    }
});


//Sort
var fsoList = document.querySelectorAll('.fso');
var folderArray = [];
var fileArray = [];

var folderIndex = 0;
var fileIndex = 0;
fsoList.forEach(function (fso) {
    if ($(fso).data('isfolder') == 'True') {
        folderArray[folderIndex++] = fso;
    } else {
        fileArray[fileIndex++] = fso;
    }
})

$('#button-sort-name').on('click', function () {
    if ($(this).html() == '<i class="fas fa-sort-alpha-down"></i>') {
        folderArray.sort(function (a, b) {
            if ($(a).data('name').toString().toLowerCase() < $(b).data('name').toString().toLowerCase())
                return -1;
            else return 1;
        });
        fileArray.sort(function (a, b) {
            if ($(a).data('name').toString().toLowerCase() < $(b).data('name').toString().toLowerCase())
                return -1;
            else return 1;
        });
        $(this).html('<i class="fas fa-sort-alpha-down-alt"></i>');
    } else {
        folderArray.sort(function (a, b) {
            if ($(a).data('name').toString().toLowerCase() > $(b).data('name').toString().toLowerCase())
                return -1;
            else return 1;
        });
        fileArray.sort(function (a, b) {
            if ($(a).data('name').toString().toLowerCase() > $(b).data('name').toString().toLowerCase())
                return -1;
            else return 1;
        });
        $(this).html('<i class="fas fa-sort-alpha-down"></i>');
    }
    $('#explorer').append(folderArray);
    $('#explorer').append(fileArray);
});


$('#button-sort-size').on('click', function () {
    if ($(this).html() == '<i class="fas fa-sort-amount-down-alt"></i>') {
        fileArray.sort(function (a, b) {
            return $(a).data('size') - $(b).data('size');
        });
        $(this).html('<i class="fas fa-sort-amount-down"></i>');
    } else {
        fileArray.sort(function (a, b) {
            return $(b).data('size') - $(a).data('size');
        });
        $(this).html('<i class="fas fa-sort-amount-down-alt"></i>');
    }
    $('#explorer').append(folderArray);
    $('#explorer').append(fileArray);
});


$('#button-sort-date').on('click', function () {
    if ($(this).html() == '<i class="fas fa-sort-numeric-down"></i>') {
        folderArray.sort(function (a, b) {
            if ($(a).data('date') < $(b).data('date'))
                return -1;
            else return 1;
        });
        fileArray.sort(function (a, b) {
            if ($(a).data('date') < $(b).data('date'))
                return -1;
            else return 1;
        });
        $(this).html('<i class="fas fa-sort-numeric-down-alt"></i>');
    } else {
        folderArray.sort(function (a, b) {
            if ($(a).data('date') > $(b).data('date'))
                return -1;
            else return 1;
        });
        fileArray.sort(function (a, b) {
            if ($(a).data('date') > $(b).data('date'))
                return -1;
            else return 1;
        });
        $(this).html('<i class="fas fa-sort-numeric-down"></i>');
    }
    $('#explorer').append(folderArray);
    $('#explorer').append(fileArray);

});


//File Ext
$('.file-ext-box').each(function () {
    let id = $(this).data('fsoid');
    let fname = $('#fso-' + id).data('name').toString();
    let ext = fname.split('.').pop().toUpperCase();
    if (ext.length <= 3) {
        $(this).css('font-size', '.8rem');
    } else if (ext.length <= 5) {
        $(this).css('font-size', '.6rem');
    } else {
        ext = '...'
    }
    $(this).html(ext);
});

//







