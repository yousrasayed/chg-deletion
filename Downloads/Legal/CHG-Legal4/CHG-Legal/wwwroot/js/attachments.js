$(document).ready(function () {
    // ========== API ENDPOINTS ==========
    const API = {
        // Company
        getCompanyAttachments: `/Companies/GetCompanyAttachments?companyId=${window.companyId}`,
        uploadCompanyAttachment: `/Companies/UploadCompanyAttachment?companyId=${window.companyId}`,
        deleteCompanyAttachment: (id) => `/Companies/DeleteCompanyAttachment?id=${id}`,
        downloadCompanyAttachment: (id) => `/Companies/DownloadCompanyAttachment?id=${id}`,

        // Shareholder
        getShareholderAttachments: (id) => `/Companies/GetShareholderAttachments?shareholderId=${id}`,
        uploadShareholderAttachment: (id) => `/Companies/UploadShareholderAttachment?shareholderId=${id}`,
        deleteShareholderAttachment: (id) => `/Companies/DeleteShareholderAttachment?id=${id}`,
        downloadShareholderAttachment: (id) => `/Companies/DownloadShareholderAttachment?id=${id}`,

        // Branch
        getBranchAttachments: (id) => `/Companies/GetBranchAttachments?branchId=${id}`,
        uploadBranchAttachment: (id) => `/Companies/UploadBranchAttachment?branchId=${id}`,
        deleteBranchAttachment: (id) => `/Companies/DeleteBranchAttachment?id=${id}`,
        downloadBranchAttachment: (id) => `/Companies/DownloadBranchAttachment?id=${id}`,

        // Board Member
        getBoardMemberAttachments: (id) => `/Companies/GetBoardMemberAttachments?memberId=${id}`,
        uploadBoardMemberAttachment: (id) => `/Companies/UploadBoardMemberAttachment?memberId=${id}`,
        deleteBoardMemberAttachment: (id) => `/Companies/DeleteBoardMemberAttachment?id=${id}`,
        downloadBoardMemberAttachment: (id) => `/Companies/DownloadBoardMemberAttachment?id=${id}`,

        // Banking
        getBankingAttachments: `/Companies/GetBankingAttachments?companyId=${window.companyId}`,
        uploadBankingAttachment: `/Companies/UploadBankingAttachment?companyId=${window.companyId}`,
        deleteBankingAttachment: (id) => `/Companies/DeleteBankingAttachment?id=${id}`,
        downloadBankingAttachment: (id) => `/Companies/DownloadBankingAttachment?id=${id}`,

        // Non-Banking
        getNonBankingAttachments: `/Companies/GetNonBankingAttachments?companyId=${window.companyId}`,
        uploadNonBankingAttachment: `/Companies/UploadNonBankingAttachment?companyId=${window.companyId}`,
        deleteNonBankingAttachment: (id) => `/Companies/DeleteNonBankingAttachment?id=${id}`,
        downloadNonBankingAttachment: (id) => `/Companies/DownloadNonBankingAttachment?id=${id}`
    };

    // ========== HELPER FUNCTIONS ==========
    function showToast(message, type) {
        type = type || 'success';
        const toastId = 'toast-' + Date.now();
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type} border-0 mb-2" role="alert" data-bs-autohide="true" data-bs-delay="3000">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="fas ${type === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle'} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;
        $('.toast-container').append(toastHtml);
        const toastEl = document.getElementById(toastId);
        if (toastEl) {
            const toast = new bootstrap.Toast(toastEl);
            toast.show();
            $(toastEl).on('hidden.bs.toast', function () { $(this).remove(); });
        }
    }

    function formatFileSize(bytes) {
        if (!bytes) return '0 B';
        const sizes = ['B', 'KB', 'MB', 'GB'];
        let i = 0;
        let size = bytes;
        while (size >= 1024 && i < sizes.length - 1) {
            size /= 1024;
            i++;
        }
        return `${size.toFixed(2)} ${sizes[i]}`;
    }

    function formatDate(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return date.toLocaleDateString('ar-EG');
    }

    function renderAttachments(containerId, attachments, downloadFn, deleteFn) {
        const $container = $(`#${containerId}`);

        if (!attachments || attachments.length === 0) {
            $container.html(`
                <div class="text-center text-muted py-4">
                    <i class="fas fa-paperclip fa-2x mb-2 d-block"></i>
                    لا توجد مرفقات حالياً
                </div>
            `);
            return;
        }

        let html = '<div class="list-group">';
        attachments.forEach(att => {
            const attachmentId = att.id || att.ID;
            const fileName = att.fileName || att.FileName || 'unknown';
            const fileType = att.fileType || att.FileType || '';
            const fileSize = att.fileSize || att.FileSize || 0;
            const uploadedAt = att.uploadedAt || att.UploadedAt;
            const description = att.description || att.Description || '';

            html += `
                <div class="list-group-item attachment-item d-flex justify-content-between align-items-center" data-id="${attachmentId}">
                    <div class="d-flex align-items-center">
                        <i class="fas ${getFileIcon(fileType)} fa-2x me-3 text-teal"></i>
                        <div>
                            <div class="fw-bold">${escapeHtml(fileName)}</div>
                            <div class="small text-muted">
                                <span class="badge bg-light text-dark me-2">${formatFileSize(fileSize)}</span>
                                <span>${formatDate(uploadedAt)}</span>
                                ${description ? `<span class="ms-2">📝 ${escapeHtml(description)}</span>` : ''}
                            </div>
                        </div>
                    </div>
                    <div>
                        <button class="btn btn-sm btn-outline-info me-1 download-attachment" data-id="${attachmentId}" title="تحميل">
                            <i class="fas fa-download"></i>
                        </button>
                        <button class="btn btn-sm btn-outline-danger delete-attachment" data-id="${attachmentId}" title="حذف">
                            <i class="fas fa-trash-alt"></i>
                        </button>
                    </div>
                </div>
            `;
        });
        html += '</div>';
        $container.html(html);

        // Attach event handlers
        $container.find('.download-attachment').on('click', function (e) {
            e.stopPropagation();
            const id = $(this).data('id');
            downloadFn(id);
        });

        $container.find('.delete-attachment').on('click', function (e) {
            e.stopPropagation();
            const id = $(this).data('id');
            if (confirm('هل أنت متأكد من حذف هذا المرفق؟')) {
                deleteFn(id);
            }
        });
    }

    function getFileIcon(fileType) {
        const type = (fileType || '').toLowerCase();
        if (type === 'pdf') return 'fa-file-pdf';
        if (['jpg', 'jpeg', 'png', 'gif', 'bmp'].includes(type)) return 'fa-file-image';
        if (['doc', 'docx'].includes(type)) return 'fa-file-word';
        if (['xls', 'xlsx'].includes(type)) return 'fa-file-excel';
        if (type === 'txt') return 'fa-file-alt';
        return 'fa-file';
    }

    function escapeHtml(text) {
        if (!text) return '';
        return String(text).replace(/[&<>]/g, function (m) {
            if (m === '&') return '&amp;';
            if (m === '<') return '&lt;';
            if (m === '>') return '&gt;';
            return m;
        });
    }

    function uploadFile(formData, url, successMsg, onSuccess) {
        showToast('جاري رفع الملف...', 'info');
        $.ajax({
            url: url,
            method: 'POST',
            headers: { 'RequestVerificationToken': window.antiForgeryToken },
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    showToast(successMsg, 'success');
                    if (onSuccess) onSuccess(response);
                } else {
                    showToast(response.message || 'حدث خطأ', 'danger');
                }
            },
            error: function (xhr) {
                let message = 'حدث خطأ أثناء رفع الملف';
                try {
                    const response = JSON.parse(xhr.responseText);
                    if (response.message) message = response.message;
                } catch (e) { }
                showToast(message, 'danger');
            }
        });
    }

    // ========== LOAD FUNCTIONS ==========
    function loadCompanyAttachments() {
        console.log('Loading company attachments...');
        $.get(API.getCompanyAttachments)
            .done(function (attachments) {
                console.log('Company attachments response:', attachments);
                renderAttachments('companyAttachmentsList', attachments,
                    (id) => window.open(API.downloadCompanyAttachment(id), '_blank'),
                    (id) => {
                        $.ajax({
                            url: API.deleteCompanyAttachment(id),
                            method: 'DELETE',
                            headers: { 'RequestVerificationToken': window.antiForgeryToken },
                            success: function () {
                                showToast('تم حذف المرفق بنجاح', 'success');
                                loadCompanyAttachments();
                            },
                            error: function () { showToast('حدث خطأ أثناء الحذف', 'danger'); }
                        });
                    }
                );
            })
            .fail(function (error) {
                console.error('Error loading company attachments:', error);
                $('#companyAttachmentsList').html('<div class="alert alert-danger">حدث خطأ في تحميل المرفقات</div>');
            });
    }

    function loadShareholderAttachments(shareholderId) {
        if (!shareholderId) {
            $('#shareholderAttachmentsList').html('<div class="text-center text-muted py-4">لا توجد مرفقات</div>');
            return;
        }

        $('#shareholderAttachmentsList').html('<div class="text-center text-muted py-4"><i class="fas fa-spinner fa-spin"></i> جاري التحميل...</div>');

        $.get(API.getShareholderAttachments(shareholderId))
            .done(function (attachments) {
                renderAttachments('shareholderAttachmentsList', attachments,
                    (id) => window.open(API.downloadShareholderAttachment(id), '_blank'),
                    (id) => {
                        $.ajax({
                            url: API.deleteShareholderAttachment(id),
                            method: 'DELETE',
                            headers: { 'RequestVerificationToken': window.antiForgeryToken },
                            success: function () {
                                showToast('تم حذف المرفق بنجاح', 'success');
                                loadShareholderAttachments(shareholderId);
                            },
                            error: function () { showToast('حدث خطأ أثناء الحذف', 'danger'); }
                        });
                    }
                );
            })
            .fail(function () {
                $('#shareholderAttachmentsList').html('<div class="alert alert-danger">حدث خطأ في تحميل المرفقات</div>');
            });
    }

    function loadBranchAttachments(branchId) {
        if (!branchId) {
            $('#branchAttachmentsList').html('<div class="text-center text-muted py-4">لا توجد مرفقات</div>');
            return;
        }

        $('#branchAttachmentsList').html('<div class="text-center text-muted py-4"><i class="fas fa-spinner fa-spin"></i> جاري التحميل...</div>');

        $.get(API.getBranchAttachments(branchId))
            .done(function (attachments) {
                renderAttachments('branchAttachmentsList', attachments,
                    (id) => window.open(API.downloadBranchAttachment(id), '_blank'),
                    (id) => {
                        $.ajax({
                            url: API.deleteBranchAttachment(id),
                            method: 'DELETE',
                            headers: { 'RequestVerificationToken': window.antiForgeryToken },
                            success: function () {
                                showToast('تم حذف المرفق بنجاح', 'success');
                                loadBranchAttachments(branchId);
                            },
                            error: function () { showToast('حدث خطأ أثناء الحذف', 'danger'); }
                        });
                    }
                );
            })
            .fail(function () {
                $('#branchAttachmentsList').html('<div class="alert alert-danger">حدث خطأ في تحميل المرفقات</div>');
            });
    }

    function loadBoardMemberAttachments(memberId) {
        console.log('loadBoardMemberAttachments called with memberId:', memberId);

        if (!memberId) {
            $('#boardMemberAttachmentsList').html('<div class="text-center text-muted py-4">لا توجد مرفقات</div>');
            return;
        }

        $('#boardMemberAttachmentsList').html('<div class="text-center text-muted py-4"><i class="fas fa-spinner fa-spin"></i> جاري التحميل...</div>');

        $.get(API.getBoardMemberAttachments(memberId))
            .done(function (attachments) {
                console.log('Response received:', attachments);
                console.log('Number of attachments:', attachments.length);

                renderAttachments('boardMemberAttachmentsList', attachments,
                    (id) => window.open(API.downloadBoardMemberAttachment(id), '_blank'),
                    (id) => {
                        $.ajax({
                            url: API.deleteBoardMemberAttachment(id),
                            method: 'DELETE',
                            headers: { 'RequestVerificationToken': window.antiForgeryToken },
                            success: function () {
                                showToast('تم حذف المرفق بنجاح', 'success');
                                loadBoardMemberAttachments(memberId);
                            },
                            error: function () { showToast('حدث خطأ أثناء الحذف', 'danger'); }
                        });
                    }
                );
            })
            .fail(function (error) {
                console.error('Error loading attachments:', error);
                $('#boardMemberAttachmentsList').html('<div class="alert alert-danger">حدث خطأ في تحميل المرفقات</div>');
            });
    }

    function loadBankingAttachments() {
        console.log('Loading banking attachments...');
        $.get(API.getBankingAttachments)
            .done(function (attachments) {
                console.log('Banking attachments response:', attachments);
                renderAttachments('bankingAttachmentsList', attachments,
                    (id) => window.open(API.downloadBankingAttachment(id), '_blank'),
                    (id) => {
                        $.ajax({
                            url: API.deleteBankingAttachment(id),
                            method: 'DELETE',
                            headers: { 'RequestVerificationToken': window.antiForgeryToken },
                            success: function () {
                                showToast('تم حذف المرفق بنجاح', 'success');
                                loadBankingAttachments();
                            },
                            error: function () { showToast('حدث خطأ أثناء الحذف', 'danger'); }
                        });
                    }
                );
            })
            .fail(function (error) {
                console.error('Error loading banking attachments:', error);
                $('#bankingAttachmentsList').html('<div class="alert alert-danger">حدث خطأ في تحميل المرفقات</div>');
            });
    }

    function loadNonBankingAttachments() {
        console.log('Loading non-banking attachments...');
        $.get(API.getNonBankingAttachments)
            .done(function (attachments) {
                console.log('Non-banking attachments response:', attachments);
                renderAttachments('nonBankingAttachmentsList', attachments,
                    (id) => window.open(API.downloadNonBankingAttachment(id), '_blank'),
                    (id) => {
                        $.ajax({
                            url: API.deleteNonBankingAttachment(id),
                            method: 'DELETE',
                            headers: { 'RequestVerificationToken': window.antiForgeryToken },
                            success: function () {
                                showToast('تم حذف المرفق بنجاح', 'success');
                                loadNonBankingAttachments();
                            },
                            error: function () { showToast('حدث خطأ أثناء الحذف', 'danger'); }
                        });
                    }
                );
            })
            .fail(function () {
                $('#nonBankingAttachmentsList').html('<div class="alert alert-danger">حدث خطأ في تحميل المرفقات</div>');
            });
    }

    // ========== EVENT HANDLERS ==========
    // Company Upload
    $('#uploadCompanyBtn').on('click', function () {
        const file = $('#companyFile')[0].files[0];
        const description = $('#companyDescription').val();
        if (!file) {
            showToast('يرجى اختيار ملف', 'danger');
            return;
        }
        const formData = new FormData();
        formData.append('file', file);
        formData.append('description', description);
        uploadFile(formData, API.uploadCompanyAttachment, 'تم رفع المرفق بنجاح', function () {
            $('#companyFile').val('');
            $('#companyDescription').val('');
            loadCompanyAttachments();
        });
    });

    // Shareholder
    $('#shareholderSelect').on('change', function () {
        const shareholderId = $(this).val();
        $('#selectedShareholderId').val(shareholderId);
        if (shareholderId) {
            $('#shareholderAttachmentsPanel').show();
            loadShareholderAttachments(shareholderId);
        } else {
            $('#shareholderAttachmentsPanel').hide();
        }
    });

    $('#uploadShareholderBtn').on('click', function () {
        const shareholderId = $('#selectedShareholderId').val();
        const file = $('#shareholderFile')[0].files[0];
        const description = $('#shareholderDescription').val();
        if (!shareholderId) {
            showToast('يرجى اختيار مساهم أولاً', 'danger');
            return;
        }
        if (!file) {
            showToast('يرجى اختيار ملف', 'danger');
            return;
        }
        const formData = new FormData();
        formData.append('file', file);
        formData.append('description', description);
        uploadFile(formData, API.uploadShareholderAttachment(shareholderId), 'تم رفع المرفق بنجاح', function () {
            $('#shareholderFile').val('');
            $('#shareholderDescription').val('');
            loadShareholderAttachments(shareholderId);
        });
    });

    // Branch
    $('#branchSelect').on('change', function () {
        const branchId = $(this).val();
        $('#selectedBranchId').val(branchId);
        if (branchId) {
            $('#branchAttachmentsPanel').show();
            loadBranchAttachments(branchId);
        } else {
            $('#branchAttachmentsPanel').hide();
        }
    });

    $('#uploadBranchBtn').on('click', function () {
        const branchId = $('#selectedBranchId').val();
        const file = $('#branchFile')[0].files[0];
        const description = $('#branchDescription').val();
        if (!branchId) {
            showToast('يرجى اختيار فرع أولاً', 'danger');
            return;
        }
        if (!file) {
            showToast('يرجى اختيار ملف', 'danger');
            return;
        }
        const formData = new FormData();
        formData.append('file', file);
        formData.append('description', description);
        uploadFile(formData, API.uploadBranchAttachment(branchId), 'تم رفع المرفق بنجاح', function () {
            $('#branchFile').val('');
            $('#branchDescription').val('');
            loadBranchAttachments(branchId);
        });
    });

    // Board Member
    $('#boardMemberSelect').on('change', function () {
        const memberId = $(this).val();
        console.log('Selected Board Member ID:', memberId);

        $('#selectedBoardMemberId').val(memberId);
        if (memberId) {
            $('#boardMemberAttachmentsPanel').show();
            loadBoardMemberAttachments(memberId);
        } else {
            $('#boardMemberAttachmentsPanel').hide();
        }
    });

    $('#uploadBoardMemberBtn').on('click', function () {
        const memberId = $('#selectedBoardMemberId').val();
        const file = $('#boardMemberFile')[0].files[0];
        const description = $('#boardMemberDescription').val();
        if (!memberId) {
            showToast('يرجى اختيار عضو أولاً', 'danger');
            return;
        }
        if (!file) {
            showToast('يرجى اختيار ملف', 'danger');
            return;
        }
        const formData = new FormData();
        formData.append('file', file);
        formData.append('description', description);
        uploadFile(formData, API.uploadBoardMemberAttachment(memberId), 'تم رفع المرفق بنجاح', function () {
            $('#boardMemberFile').val('');
            $('#boardMemberDescription').val('');
            loadBoardMemberAttachments(memberId);
        });
    });

    // Banking
    $('#uploadBankingBtn').on('click', function () {
        const file = $('#bankingFile')[0].files[0];
        const description = $('#bankingDescription').val();
        if (!file) {
            showToast('يرجى اختيار ملف', 'danger');
            return;
        }
        const formData = new FormData();
        formData.append('file', file);
        formData.append('description', description);
        uploadFile(formData, API.uploadBankingAttachment, 'تم رفع المرفق بنجاح', function () {
            $('#bankingFile').val('');
            $('#bankingDescription').val('');
            loadBankingAttachments();
        });
    });

    // Non-Banking
    $('#uploadNonBankingBtn').on('click', function () {
        const file = $('#nonBankingFile')[0].files[0];
        const description = $('#nonBankingDescription').val();
        if (!file) {
            showToast('يرجى اختيار ملف', 'danger');
            return;
        }
        const formData = new FormData();
        formData.append('file', file);
        formData.append('description', description);
        uploadFile(formData, API.uploadNonBankingAttachment, 'تم رفع المرفق بنجاح', function () {
            $('#nonBankingFile').val('');
            $('#nonBankingDescription').val('');
            loadNonBankingAttachments();
        });
    });

    // ========== INITIAL LOAD ==========
    loadCompanyAttachments();
    loadBankingAttachments();
    loadNonBankingAttachments();
});