// wwwroot/js/companies.js
$(document).ready(function () {
    // ========== API CONFIGURATION ==========
    const API = {
        getCompanies: '/Companies/GetCompanies',
        getCompany: '/Companies/GetCompany',
        saveCompany: '/Companies/SaveCompany',
        deleteCompany: '/Companies/DeleteCompany',
        getShareValue: '/Companies/GetShareValue',
        getBankGroups: '/Companies/GetBankGroups'
    };

    // ========== GLOBAL STATE ==========
    let companies = [];
    let editingCompanyId = null;
    let shareholders = [];
    let branches = [];
    let boardMembers = [];
    let bankSignatures = [];
    let nonBankSignatures = [];
    
    window.currentMissingFields = [];
    // ========== ID SEQUENCE COUNTERS ==========
    let nextShareholderId = -1;
    let nextBranchId = -1;
    let nextBoardMemberId = -1;
    let nextBankSignatureId = -1;
    let nextNonBankSignatureId = -1;

    function getNextId(counterName) {
        if (window[counterName] === -1) {
            let maxId = 0;
            switch (counterName) {
                case 'nextShareholderId':
                    maxId = shareholders.reduce((max, s) => Math.max(max, s.shareholderId || 0), 0);
                    break;
                case 'nextBranchId':
                    maxId = branches.reduce((max, b) => Math.max(max, b.branchId || 0), 0);
                    break;
                case 'nextBoardMemberId':
                    maxId = boardMembers.reduce((max, m) => Math.max(max, m.memberId || 0), 0);
                    break;
                case 'nextBankSignatureId':
                    maxId = bankSignatures.reduce((max, bg) => Math.max(max, bg.groupId || 0), 0);
                    break;
                case 'nextNonBankSignatureId':
                    maxId = nonBankSignatures.reduce((max, nb) => Math.max(max, nb.authorizationId || 0), 0);
                    break;
            }
            window[counterName] = maxId;
        }
        window[counterName]++;
        return window[counterName];
    }

    function resetIdCounters() {
        nextShareholderId = -1;
        nextBranchId = -1;
        nextBoardMemberId = -1;
        nextBankSignatureId = -1;
        nextNonBankSignatureId = -1;
    }

    // ========== HELPER FUNCTIONS FOR DATES ==========
    function cleanDate(dateString) {
        if (!dateString) return null;
        if (dateString.includes('T')) {
            return dateString.split('T')[0];
        }
        return dateString;
    }

    function formatDate(dateString) {
        if (!dateString) return '';
        let clean = dateString;
        if (dateString.includes('T')) {
            clean = dateString.split('T')[0];
        }
        if (clean && clean.match(/^\d{4}-\d{2}-\d{2}$/)) {
            const parts = clean.split('-');
            return `${parts[2]}/${parts[1]}/${parts[0]}`;
        }
        return clean || '';
    }

    function formatBoardDate(dateString) {
        if (!dateString) return '';
        let clean = dateString;
        if (dateString.includes('T')) {
            clean = dateString.split('T')[0];
        }
        if (clean && clean.match(/^\d{4}-\d{2}-\d{2}$/)) {
            const parts = clean.split('-');
            return `${parts[1]}/${parts[0]}`;
        }
        return clean || '';
    }

    function formatDateForInput(dateString) {
        if (!dateString) return '';
        return cleanDate(dateString) || '';
    }

    function formatDateForSave(dateValue) {
        if (!dateValue) return null;
        if (dateValue.includes('T')) {
            return dateValue.split('T')[0];
        }
        return dateValue;
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

    // ========== AJAX REQUEST HELPER ==========
    function ajaxRequest(url, method, data, successCallback, errorCallback) {
        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': window.antiForgeryToken },
            data: data ? JSON.stringify(data) : null,
            success: function (response) {
                if (response.success === false) {
                    if (errorCallback) errorCallback(response.message);
                    else showToast(response.message || 'حدث خطأ', 'danger');
                } else {
                    if (successCallback) successCallback(response);
                }
            },
            error: function (xhr, status, error) {
                console.error(`Error from ${url}:`, { status, error, response: xhr.responseText });
                let message = 'حدث خطأ في الاتصال';
                try {
                    const response = JSON.parse(xhr.responseText);
                    if (response.message) message = response.message;
                } catch (e) {
                    if (xhr.status === 401) message = 'يرجى تسجيل الدخول أولاً';
                    else if (xhr.status === 404) message = 'العنوان غير موجود';
                    else if (xhr.status === 500) message = 'خطأ في الخادم';
                }
                if (errorCallback) errorCallback(message);
                else showToast(message, 'danger');
            }
        });
    }

    // ========== LOAD COMPANIES ==========
    function loadCompanies() {
        ajaxRequest(API.getCompanies, 'GET', null, function (response) {
            companies = response;
            renderCompaniesTable();
        });
    }

    function renderCompaniesTable() {
        if (!companies || companies.length === 0) {
            $('#companiesTableBody').html(`
            <tr>
                <td colspan="8" class="text-center text-muted py-4">
                    <i class="fas fa-building fa-2x mb-2 d-block"></i>
                    لا توجد شركات حالياً، اضغط على "إضافة شركة جديدة" للبدء
                </td>
            </tr>
        `);
            return;
        }
        let html = '';

        companies.forEach(function (company, index) {
            // Highlight search term if exists
            let companyName = company.name || '';
            let commercialRegNo = company.commercialRegNo || '';

            if (currentSearchTerm && currentFilterType === 'all') {
                const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
                companyName = companyName.replace(regex, '<span class="search-highlight">$1</span>');
                commercialRegNo = commercialRegNo.replace(regex, '<span class="search-highlight">$1</span>');
            } else if (currentSearchTerm && currentFilterType === 'name') {
                const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
                companyName = companyName.replace(regex, '<span class="search-highlight">$1</span>');
            } else if (currentSearchTerm && currentFilterType === 'commercial') {
                const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
                commercialRegNo = commercialRegNo.replace(regex, '<span class="search-highlight">$1</span>');
            }

            // Get status badge
            let statusBadge = getStatusBadge(company);

            html += `
            <tr>
                <td class="text-center">${index + 1}</td>
                <td class="text-center">${companyName || '-'}</td>
                <td class="text-center">${commercialRegNo || '-'}</td>
                <td class="text-center">${statusBadge}</td>
                <td class="text-center">${(company.authorizedCapital || 0).toLocaleString()} ${(company.authorizedCapital ? 'ج.م' : '')}</td>
                <td class="text-center">${company.shareholders?.length || 0}</td>
                <td class="text-center">${company.branches?.length || 0}</td>
                <td class="text-center">
                    <div class="action-buttons">
                        <button class="btn-action btn-view" onclick="viewCompanyDetails(${company.companyId})" title="عرض التفاصيل">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn-action btn-edit" onclick="editCompany(${company.companyId})" title="تعديل">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn-action btn-attach" onclick="manageAttachments(${company.companyId})" title="إدارة المرفقات">
                            <i class="fas fa-paperclip"></i>
                        </button>
                        <button class="btn-action btn-delete" onclick="deleteCompany(${company.companyId})" title="حذف">
                            <i class="fas fa-trash-alt"></i>
                        </button>
                    </div>
                </td>
             </tr>
        `;
        });

        $('#companiesTableBody').html(html);
    }
    //function renderCompaniesTable() {
    //    if (!companies || companies.length === 0) {
    //        $('#companiesTableBody').html(`
    //            <tr>
    //                <td colspan="8" class="text-center text-muted py-4">
    //                    <i class="fas fa-building fa-2x mb-2 d-block"></i>
    //                    لا توجد شركات حالياً، اضغط على "إضافة شركة جديدة" للبدء
    //                </td>
    //            </tr>
    //        `);
    //        return;
    //    }
    //    let html = '';

    //    companies.forEach(function (company, index) {
    //        // Highlight search term if exists
    //        let companyName = company.name || '';
    //        let commercialRegNo = company.commercialRegNo || '';
    //        let headOfficeAddress = company.headOfficeAddress || '';

    //        if (currentSearchTerm && currentFilterType === 'all') {
    //            const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
    //            companyName = companyName.replace(regex, '<span class="search-highlight">$1</span>');
    //            commercialRegNo = commercialRegNo.replace(regex, '<span class="search-highlight">$1</span>');
    //            headOfficeAddress = headOfficeAddress.replace(regex, '<span class="search-highlight">$1</span>');
    //        } else if (currentSearchTerm && currentFilterType === 'name') {
    //            const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
    //            companyName = companyName.replace(regex, '<span class="search-highlight">$1</span>');
    //        } else if (currentSearchTerm && currentFilterType === 'commercial') {
    //            const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
    //            commercialRegNo = commercialRegNo.replace(regex, '<span class="search-highlight">$1</span>');
    //        } else if (currentSearchTerm && currentFilterType === 'address') {
    //            const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
    //            headOfficeAddress = headOfficeAddress.replace(regex, '<span class="search-highlight">$1</span>');
    //        }

    //        // Get status badge
    //        let statusBadge = getStatusBadge(company);
    //        //let html = '';
    //        html += `
    //        <tr>
    //            <td class="text-center">${index + 1}${statusBadge ? '<br>' + statusBadge : ''}</td>
    //            <td class="text-center">${companyName || '-'}</td>
    //            <td class="text-center">${commercialRegNo || '-'}</td>
    //            <td class="text-center">${headOfficeAddress || '-'}</td>
    //            <td class="text-center">${(company.authorizedCapital || 0).toLocaleString()} ${(company.authorizedCapital ? 'ج.م' : '')}</td>
    //            <td class="text-center">${company.shareholders?.length || 0}</td>
    //            <td class="text-center">${company.branches?.length || 0}</td>
    //            <td class="text-center">
    //                <div class="action-buttons">
    //                    <button class="btn-action btn-view" onclick="viewCompanyDetails(${company.companyId})" title="عرض التفاصيل">
    //                        <i class="fas fa-eye"></i>
    //                    </button>
    //                    <button class="btn-action btn-edit" onclick="editCompany(${company.companyId})" title="تعديل">
    //                        <i class="fas fa-edit"></i>
    //                    </button>
    //                    <button class="btn-action btn-attach" onclick="manageAttachments(${company.companyId})" title="إدارة المرفقات">
    //                        <i class="fas fa-paperclip"></i>
    //                    </button>
    //                    <button class="btn-action btn-delete" onclick="deleteCompany(${company.companyId})" title="حذف">
    //                        <i class="fas fa-trash-alt"></i>
    //                    </button>
    //                </div>
    //            </td>
    //         </tr>
    //    `;
    //    });
  
    //    $('#companiesTableBody').html(html);
    //}

    // ========== VIEW COMPANY DETAILS ==========
    window.viewCompanyDetails = function (id) {
        ajaxRequest(API.getCompany + '?id=' + id, 'GET', null, function (company) {
            showCompanyDetailsModal(company);
        });
    };

    function showCompanyDetailsModal(company) {
        let detailsHtml = buildCompanyDetailsHtml(company);

        $('#detailsModal').remove();
        const modalHtml = `
        <div class="modal fade" id="detailsModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-xl modal-dialog-scrollable">
                <div class="modal-content" style="border-radius: 20px; overflow: hidden;">
                    <div class="modal-header" style="background: linear-gradient(135deg, #91d3d1 0%, #7bc6c3 50%, #65b8b5 100%); color: white; border-bottom: none;">
                        <h5 class="modal-title">
                            <i class="fas fa-building me-2"></i>تفاصيل الشركة: ${escapeHtml(company.name || company.companyName || '-')}
                        </h5>
                        <div>
                            <button type="button" class="btn btn-sm btn-light me-2" id="printDetailsBtn" title="طباعة" style="background: rgba(255,255,255,0.2); border: none; color: white; border-radius: 10px;">
                                <i class="fas fa-print"></i> طباعة
                            </button>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                        </div>
                    </div>
                    <div class="modal-body" id="companyDetailsPrintArea" style="background-color: #f0f7f7;">
                        ${detailsHtml}
                    </div>
                    <div class="modal-footer" style="background: #f8fbfb; border-top: 1px solid #e0eceb;">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" style="background: #6c757d; border: none; border-radius: 10px;">إغلاق</button>
                        <button type="button" class="btn" onclick="editCompany(${company.companyId || company.id}); $('#detailsModal').modal('hide');" style="background: linear-gradient(135deg, #91d3d1, #7bc6c3, #65b8b5); border: none; color: white; border-radius: 10px;">
                            <i class="fas fa-edit me-1"></i>تعديل الشركة
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
        $('body').append(modalHtml);
        $('#detailsModal').modal('show');

        // ========== PRINT FUNCTIONALITY ==========
        $('#printDetailsBtn').off('click').on('click', function () {
            const allContent = $('#companyDetailsPrintArea').html();
            if (!allContent) {
                showToast('حدث خطأ: لا توجد بيانات للطباعة', 'danger');
                return;
            }
            showToast('جاري تجهيز صفحة الطباعة...', 'info');

            const $form = $('<form>', {
                action: '/Companies/PrintCompanyDetails',
                method: 'POST',
                target: '_blank'
            });

            $form.append($('<input>', {
                type: 'hidden',
                name: '__RequestVerificationToken',
                value: window.antiForgeryToken
            }));

            $form.append($('<input>', {
                type: 'hidden',
                name: 'companyName',
                value: company.name || company.companyName || 'شركة'
            }));

            $form.append($('<input>', {
                type: 'hidden',
                name: 'allDetailsHtml',
                value: allContent
            }));

            $('body').append($form);
            $form.submit();
            $form.remove();
        });

        $('#detailsModal').on('hidden.bs.modal', function () { $('#detailsModal').remove(); });
    }

    function buildCompanyDetailsHtml(company) {
        let html = '';

        // ========== SECTION 1: COMPANY BASIC INFO ==========
        html += `
    <div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-building me-2"></i>بيانات الشركة الأساسية
        </h5>
        <div class="row mt-3">
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">اسم الشركة:</strong> <span style="color: #2c5a58;">${escapeHtml(company.name || company.companyName || '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">العنوان الرئيسي:</strong> <span style="color: #2c5a58;">${escapeHtml(company.headOfficeAddress || company.mainAddress || '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">رقم السجل التجاري:</strong> <span style="color: #2c5a58;">${escapeHtml(company.commercialRegNo || company.commercialRecord || '-')}</span></div>
        </div>
    </div>
`;

        // ========== SECTION 2: REGISTRATION DATA ==========
        html += `
    <div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-registered me-2"></i>بيانات القيد والتسجيل
        </h5>
        <div class="row mt-3">
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">تاريخ انتهاء قيد التسجيل:</strong> <span style="color: #2c5a58;">${formatDate(company.registrationExpiry) || formatDate(company.registrationExpiryDate) || '-'}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">تاريخ التسجيل:</strong> <span style="color: #2c5a58;">${formatDate(company.registrationDate) || formatDate(company.commercialRecordDate) || '-'}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">إشعار قبل انتهاء القيد:</strong> <span style="color: #2c5a58;">${company.registrationNotificationPeriod || company.registrationExpiryNotificationDays ? (company.registrationNotificationPeriod || company.registrationExpiryNotificationDays) + ' يوم' : '-'}</span></div>
        </div>
    </div>
`;

        // ========== SECTION 3: TAX DATA ==========
        html += `
    <div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-chart-line me-2"></i>البيانات الضريبية
        </h5>
        <div class="row mt-3">
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">رقم البطاقة الضريبية:</strong> <span style="color: #2c5a58;">${escapeHtml(company.taxCardNumber || company.taxCard || '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">تاريخ تجديد البطاقة الضريبية:</strong> <span style="color: #2c5a58;">${formatDate(company.cardRenewalDate) || formatDate(company.taxCardRenewalDate) || '-'}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">إشعار قبل تجديد البطاقة:</strong> <span style="color: #2c5a58;">${company.cardRenewalDue || company.taxCardNotificationDays ? (company.cardRenewalDue || company.taxCardNotificationDays) + ' يوم' : '-'}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">ضريبة القيمة المضافة:</strong> <span style="color: #2c5a58;">${company.vat || company.vatNumber || '-'}</span></div>
        </div>
    </div>
`;

        // ========== SECTION 4: AUDITOR DATA ==========
        html += `
    <div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-user-check me-2"></i>مراقب الحسابات
        </h5>
        <div class="row mt-3">
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">اسم مراقب الحسابات:</strong> <span style="color: #2c5a58;">${escapeHtml(company.accountingAuditor || company.auditor || '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">تاريخ تعيين مراقب الحسابات:</strong> <span style="color: #2c5a58;">${formatDate(company.accountingAuditorHiringDate) || formatDate(company.auditorAppointmentDate) || '-'}</span></div>
        </div>
    </div>
`;

        // ========== SECTION 5: CAPITAL DATA ==========
        html += `
    <div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-money-bill-wave me-2"></i>رأس المال
        </h5>
        <div class="row mt-3">
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">رأس المال المرخص به:</strong> <span style="color: #2c5a58;">${company.authorizedCapital || 0} جنيه</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">تاريخ التأشير:</strong> <span style="color: #2c5a58;">${formatDate(company.authorizedCapitalDate) || formatDate(company.authorizedCapitalRecordDate) || '-'}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">رأس المال المصدر:</strong> <span style="color: #2c5a58;">${company.issuedCapital || 0} جنيه</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">تاريخ التأشير:</strong> <span style="color: #2c5a58;">${formatDate(company.issuedCapitalDate) || formatDate(company.issuedCapitalRecordDate) || '-'}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">رأس المال المدفوع:</strong> <span style="color: #2c5a58;">${company.paidUpCapital || company.paidCapital || 0} جنيه</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">تاريخ التأشير:</strong> <span style="color: #2c5a58;">${formatDate(company.paidUpCapitalDate) || formatDate(company.paidCapitalRecordDate) || '-'}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">قيمة السهم الواحد:</strong> <span style="color: #2c5a58;">${company.shareValue || company.fixedShareValue || 1.00} جنيه</span></div>
        </div>
    </div>
`;

        // ========== SECTION 6: SHAREHOLDERS ==========
        const shareholdersList = company.shareholders || [];
        if (shareholdersList.length > 0) {
            const activeShareholders = shareholdersList.filter(s => !s.endDate);
            const exitedShareholders = shareholdersList.filter(s => s.endDate);
            const shareValue = company.shareValue || company.fixedShareValue || 1.00;

            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-users me-2"></i>المساهمين
        </h5>`;

            if (activeShareholders.length > 0) {
                html += `<h6 class="mt-2" style="color: #28a745; border-right: 3px solid #28a745; padding-right: 10px;">المساهمين النشطين (${activeShareholders.length})</h6>
            <div class="table-responsive mt-3">
                <table class="table table-sm" style="width: 100%; border-collapse: collapse; background: white; border-radius: 10px; overflow: hidden;">
                    <thead>
                        <tr style="background: linear-gradient(135deg, #91d3d1, #7bc6c3, #65b8b5); color: white;">
                            <th style="padding: 12px; text-align: center;">#</th>
                            <th style="padding: 12px; text-align: center;">الاسم</th>
                            <th style="padding: 12px; text-align: center;">الصفة</th>
                            <th style="padding: 12px; text-align: center;">أسهم مكتتبة</th>
                            <th style="padding: 12px; text-align: center;">أسهم ممتازة</th>
                            <th style="padding: 12px; text-align: center;">أسهم مؤسس</th>
                            <th style="padding: 12px; text-align: center;">النسبة</th>
                            <th style="padding: 12px; text-align: center;">تاريخ الدخول</th>
                        </tr>
                    </thead>
                    <tbody>`;
                activeShareholders.forEach((s, idx) => {
                    let subscribed = s.subscribedShareCount || s.subscribedShares || 0;
                    let excellent = s.excellentShareCount || s.preferredShares || 0;
                    let founder = s.founderShareCount || s.founderShares || 0;
                    let totalShares = subscribed + excellent + founder;
                    let totalCapital = company.authorizedCapital || company.issuedCapital || company.paidUpCapital || 1;
                    let percentage = ((totalShares * shareValue) / totalCapital * 100).toFixed(2);
                    html += `<tr style="border-bottom: 1px solid #e0eceb;">
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${idx + 1}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(s.shareName || s.name)}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(s.role || s.title || '-')}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${subscribed}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${excellent}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${founder}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${percentage}%</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${formatDate(s.startDate || s.entryDate)}</td>
                </tr>`;
                });
                html += `</tbody>
                </table>
            </div>`;
            }

            if (exitedShareholders.length > 0) {
                html += `<h6 class="mt-3" style="color: #dc3545; border-right: 3px solid #dc3545; padding-right: 10px;">المساهمين السابقين (${exitedShareholders.length})</h6>
            <div class="table-responsive mt-3">
                <table class="table table-sm" style="width: 100%; border-collapse: collapse; background: white; border-radius: 10px; overflow: hidden;">
                    <thead>
                        <tr style="background: linear-gradient(135deg, #91d3d1, #7bc6c3, #65b8b5); color: white;">
                            <th style="padding: 12px; text-align: center;">#</th>
                            <th style="padding: 12px; text-align: center;">الاسم</th>
                            <th style="padding: 12px; text-align: center;">الصفة</th>
                            <th style="padding: 12px; text-align: center;">أسهم مكتتبة</th>
                            <th style="padding: 12px; text-align: center;">أسهم ممتازة</th>
                            <th style="padding: 12px; text-align: center;">أسهم مؤسس</th>
                            <th style="padding: 12px; text-align: center;">النسبة</th>
                            <th style="padding: 12px; text-align: center;">تاريخ الدخول</th>
                            <th style="padding: 12px; text-align: center;">تاريخ الخروج</th>
                        </tr>
                    </thead>
                    <tbody>`;
                exitedShareholders.forEach((s, idx) => {
                    let subscribed = s.subscribedShareCount || s.subscribedShares || 0;
                    let excellent = s.excellentShareCount || s.preferredShares || 0;
                    let founder = s.founderShareCount || s.founderShares || 0;
                    let totalShares = subscribed + excellent + founder;
                    let totalCapital = company.authorizedCapital || company.issuedCapital || company.paidUpCapital || 1;
                    let percentage = ((totalShares * shareValue) / totalCapital * 100).toFixed(2);
                    html += `<tr style="border-bottom: 1px solid #e0eceb;">
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${idx + 1}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(s.shareName || s.name)}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(s.role || s.title || '-')}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${subscribed}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${excellent}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${founder}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${percentage}%</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${formatDate(s.startDate || s.entryDate)}</td>
                    <td style="padding: 10px; text-align: center; color: #2c5a58;">${formatDate(s.endDate || s.exitDate)}</td>
                </tr>`;
                });
                html += `</tbody>
                </table>
            </div>`;
            }
            html += `</div>`;
        } else {
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);"><h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px;"><i class="fas fa-users me-2"></i>المساهمين</h5><p class="text-muted" style="color: #7bc6c3;">لا يوجد مساهمين</p></div>`;
        }

        // ========== SECTION 7: BRANCHES ==========
        const branchesList = company.branches || [];
        if (branchesList.length > 0) {
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-code-branch me-2"></i>الفروع
        </h5>
        <div class="table-responsive">
            <table class="table table-sm" style="width: 100%; border-collapse: collapse; background: white; border-radius: 10px; overflow: hidden;">
                <thead>
                    <tr style="background: linear-gradient(135deg, #91d3d1, #7bc6c3, #65b8b5); color: white;">
                        <th style="padding: 12px; text-align: center;">#</th>
                        <th style="padding: 12px; text-align: center;">اسم الفرع</th>
                        <th style="padding: 12px; text-align: center;">العنوان</th>
                        <th style="padding: 12px; text-align: center;">رقم السجل</th>
                        <th style="padding: 12px; text-align: center;">تاريخ انتهاء القيد</th>
                        <th style="padding: 12px; text-align: center;">إشعار قبل الانتهاء</th>
                    </tr>
                </thead>
                <tbody>`;
            branchesList.forEach((b, idx) => {
                html += `<tr style="border-bottom: 1px solid #e0eceb;">
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${idx + 1}</td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(b.name)}</td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(b.address || '-')}</td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(b.registrationNumber || b.record || '-')}</td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${formatDate(b.expiryDate) || formatDate(b.effectiveDate) || '-'}</td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${b.registrationNotification || b.expiryNotificationDays ? (b.registrationNotification || b.expiryNotificationDays) + ' يوم' : '-'}</td>
            </tr>`;
            });
            html += `</tbody>
            </table>
        </div></div>`;
        } else {
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);"><h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px;"><i class="fas fa-code-branch me-2"></i>الفروع</h5><p class="text-muted" style="color: #7bc6c3;">لا يوجد فروع</p></div>`;
        }

        // ========== SECTION 8: MEETINGS ==========
        const meetings = company.meetings || {};
        if (Object.keys(meetings).length > 0) {
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-calendar-alt me-2"></i>الاجتماعات
        </h5>`;

            // Ordinary Meeting
            const ordinary = meetings.ordinaryMeeting || {};
            html += `<h6 class="mt-3" style="color: #4a9f9c; border-right: 3px solid #7bc6c3; padding-right: 10px;">الجمعية العامة العادية</h6>
        <div class="row mt-2">
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">مكان الانعقاد:</strong> <span style="color: #2c5a58;">${ordinary.places ? ordinary.places.join('، ') : (meetings.ordinaryLocation ? meetings.ordinaryLocation.join('، ') : '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">العنوان:</strong> <span style="color: #2c5a58;">${escapeHtml(ordinary.address || meetings.ordinaryAddress || '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">نصاب الاجتماع:</strong> <span style="color: #2c5a58;">${ordinary.validityValue || meetings.ordinaryQuorum || '-'}%</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">آلية التصويت:</strong> <span style="color: #2c5a58;">${ordinary.votingMechanism === 'majority' ? 'أغلبية' : ordinary.votingMechanism === 'cumulative' ? 'تراكمي' : (meetings.ordinaryVoting === 'majority' ? 'أغلبية' : meetings.ordinaryVoting === 'cumulative' ? 'تراكمي' : '-')}</span></div>
        </div>`;

            // Extraordinary Meeting
            const extraordinary = meetings.extraordinaryMeeting || {};
            html += `<h6 class="mt-3" style="color: #4a9f9c; border-right: 3px solid #7bc6c3; padding-right: 10px;">الجمعية العامة الغير عادية</h6>
        <div class="row mt-2">
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">مكان الانعقاد:</strong> <span style="color: #2c5a58;">${extraordinary.places ? extraordinary.places.join('، ') : (meetings.extraordinaryLocation ? meetings.extraordinaryLocation.join('، ') : '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">العنوان:</strong> <span style="color: #2c5a58;">${escapeHtml(extraordinary.address || meetings.extraordinaryAddress || '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">نصاب الاجتماع:</strong> <span style="color: #2c5a58;">${extraordinary.validityValue || meetings.extraordinaryQuorum || '-'}%</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">آلية التصويت:</strong> <span style="color: #2c5a58;">${extraordinary.votingMechanism === 'majority' ? 'أغلبية' : extraordinary.votingMechanism === 'cumulative' ? 'تراكمي' : (meetings.extraordinaryVoting === 'majority' ? 'أغلبية' : meetings.extraordinaryVoting === 'cumulative' ? 'تراكمي' : '-')}</span></div>
        </div>`;

            // Board Meeting
            const board = meetings.boardMeeting || {};
            html += `<h6 class="mt-3" style="color: #4a9f9c; border-right: 3px solid #7bc6c3; padding-right: 10px;">اجتماع مجلس الإدارة</h6>
        <div class="row mt-2">
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">مكان الانعقاد:</strong> <span style="color: #2c5a58;">${board.places ? board.places.join('، ') : (meetings.boardLocation ? meetings.boardLocation.join('، ') : '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">العنوان:</strong> <span style="color: #2c5a58;">${escapeHtml(board.address || meetings.boardAddress || '-')}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">نصاب الاجتماع:</strong> <span style="color: #2c5a58;">${board.validityValue || meetings.boardQuorum || '-'}%</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">آلية التصويت:</strong> <span style="color: #2c5a58;">${board.votingMechanism === 'majority' ? 'أغلبية' : board.votingMechanism === 'cumulative' ? 'تراكمي' : (meetings.boardVoting === 'majority' ? 'أغلبية' : meetings.boardVoting === 'cumulative' ? 'تراكمي' : '-')}</span></div>
        </div>`;
            html += `</div>`;
        }

        // ========== SECTION 9: BOARD MEMBERS ==========
        const boardMembersList = company.boardMembers || [];
        if (boardMembersList.length > 0) {
            const boardSettings = company.boardSettings || {};
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-user-tie me-2"></i>أعضاء مجلس الإدارة
        </h5>
        <div class="row mb-3">
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">مدة المجلس:</strong> <span style="color: #2c5a58;">${boardSettings.duration || company.boardDuration || '-'} ${company.boardDurationUnit === 'years' ? 'سنوات' : company.boardDurationUnit === 'months' ? 'شهور' : ''}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">تاريخ بداية المجلس:</strong> <span style="color: #2c5a58;">${formatBoardDate(boardSettings.startDate || company.boardStartDate) || '-'}</span></div>
            <div class="col-md-4 mb-2"><strong style="color: #4a9f9c;">إشعار قبل انتهاء المجلس:</strong> <span style="color: #2c5a58;">${boardSettings.notificationPeriod || company.boardExpiryNotificationDays ? (boardSettings.notificationPeriod || company.boardExpiryNotificationDays) + ' يوم' : '-'}</span></div>
        </div>
        <div class="table-responsive">
            <table class="table table-sm" style="width: 100%; border-collapse: collapse; background: white; border-radius: 10px; overflow: hidden;">
                <thead>
                    <tr style="background: linear-gradient(135deg, #91d3d1, #7bc6c3, #65b8b5); color: white;">
                        <th style="padding: 12px; text-align: center;">#</th>
                        <th style="padding: 12px; text-align: center;">اسم العضو</th>
                        <th style="padding: 12px; text-align: center;">الصفة</th>
                    </tr>
                </thead>
                <tbody>`;
            boardMembersList.forEach((m, idx) => {
                let titlesList = Array.isArray(m.position) ? m.position.join('، ') : (m.position || m.title || '-');
                html += `<tr style="border-bottom: 1px solid #e0eceb;">
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${idx + 1}</td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(m.name)}</td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(titlesList)}</td>
            </tr>`;
            });
            html += `</tbody>
            </table>
        </div></div>`;
        } else {
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);"><h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px;"><i class="fas fa-user-tie me-2"></i>أعضاء مجلس الإدارة</h5><p class="text-muted" style="color: #7bc6c3;">لا يوجد أعضاء مجلس إدارة</p></div>`;
        }

        // ========== SECTION 10: BANK SIGNATURES ==========
        const bankSignaturesList = company.bankSignatures || [];
        if (bankSignaturesList.length > 0) {
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-university me-2"></i>صلاحيات التوقيع البنكية
        </h5>`;

            if (company.bankSignaturesNotes) {
                html += `<div class="alert mt-2" style="background: linear-gradient(135deg, #f0f9fa, #e8f5f6); color: #4a9f9c; border-right: 4px solid #7bc6c3; border-radius: 10px;"><i class="fas fa-sticky-note me-2"></i>${escapeHtml(company.bankSignaturesNotes)}</div>`;
            }

            html += `
        <div class="table-responsive">
            <table class="table table-sm" style="width: 100%; border-collapse: collapse; background: white; border-radius: 10px; overflow: hidden;">
                <thead>
                    <tr style="background: linear-gradient(135deg, #91d3d1, #7bc6c3, #65b8b5); color: white;">
                        <th style="padding: 12px; text-align: center;">#</th>
                        <th style="padding: 12px; text-align: center;">المجموعة</th>
                        <th style="padding: 12px; text-align: center;">الأشخاص المصرح لهم</th>
                        <th style="padding: 12px; text-align: center;">الملاحظات</th>
                    </tr>
                </thead>
                <tbody>
        `;

            bankSignaturesList.forEach((s, idx) => {
                let membersDisplay = '';
                if (s.members && s.members.length > 0) {
                    membersDisplay = '<ul style="margin: 0; padding-right: 20px; text-align: right; color: #2c5a58;">';
                    s.members.forEach(member => {
                        membersDisplay += `<li>${escapeHtml(member)}</li>`;
                    });
                    membersDisplay += '</ul>';
                } else if (s.authorizedPersons) {
                    let personsArray = s.authorizedPersons.split(',').map(p => p.trim());
                    membersDisplay = '<ul style="margin: 0; padding-right: 20px; text-align: right; color: #2c5a58;">';
                    personsArray.forEach(member => {
                        membersDisplay += `<li>${escapeHtml(member)}</li>`;
                    });
                    membersDisplay += '</ul>';
                } else {
                    membersDisplay = '-';
                }

                html += `<tr style="border-bottom: 1px solid #e0eceb;">
                <td style="padding: 10px; text-align: center; vertical-align: middle; color: #2c5a58;">${idx + 1}</td>
                <td style="padding: 10px; text-align: center; vertical-align: middle; color: #2c5a58;">${escapeHtml(s.groupName)}</span></td>
                <td style="text-align: right; padding: 10px;">${membersDisplay}</td>
                <td style="padding: 10px; text-align: center; vertical-align: middle; color: #2c5a58;">${escapeHtml(s.note || '-')}</td>
            </tr>`;
            });
            html += `</tbody>
            </table>
        </div></div>`;
        } else {
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);"><h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px;"><i class="fas fa-university me-2"></i>صلاحيات التوقيع البنكية</h5><p class="text-muted" style="color: #7bc6c3;">لا توجد صلاحيات توقيع بنكية</p></div>`;
        }

        // ========== SECTION 11: NON-BANK SIGNATURES ==========
        const nonBankSignaturesList = company.nonBankSignatures || [];
        if (nonBankSignaturesList.length > 0) {
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
        <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px; margin-bottom: 20px;">
            <i class="fas fa-file-signature me-2"></i>صلاحيات التوقيع غير البنكية
        </h5>`;

            if (company.nonBankSignaturesNotes) {
                html += `<div class="alert mt-2" style="background: linear-gradient(135deg, #f0f9fa, #e8f5f6); color: #4a9f9c; border-right: 4px solid #7bc6c3; border-radius: 10px;">
                <i class="fas fa-sticky-note me-2"></i>${escapeHtml(company.nonBankSignaturesNotes)}
            </div>`;
            }

            html += `
        <div class="table-responsive">
            <table class="table table-sm" style="width: 100%; border-collapse: collapse; background: white; border-radius: 10px; overflow: hidden;">
                <thead>
                    <tr style="background: linear-gradient(135deg, #91d3d1, #7bc6c3, #65b8b5); color: white;">
                        <th style="padding: 12px; text-align: center;">#</th>
                        <th style="padding: 12px; text-align: center;">الاسم الكامل</th>
                        <th style="padding: 12px; text-align: center;">صادر لصالح</th>
                        <th style="padding: 12px; text-align: center;">تاريخ الانتهاء</th>
                        <th style="padding: 12px; text-align: center;">إشعار قبل الانتهاء</th>
                    </tr>
                </thead>
                <tbody>
        `;

            nonBankSignaturesList.forEach((s, idx) => {
                html += `<tr style="border-bottom: 1px solid #e0eceb;">
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${idx + 1}</td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(s.name || s.fullName)}</span></td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${escapeHtml(s.issuedTo || '-')}</span></td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${formatDate(s.expiryDate)}</span></td>
                <td style="padding: 10px; text-align: center; color: #2c5a58;">${s.notificationPerid ? s.notificationPerid + ' يوم' : '-'}</span></td>
            </tr>`;
            });

            html += `</tbody>
            </table>
        </div>`;

            if (nonBankSignaturesList.some(s => s.authorizationDetails || s.delegationContent)) {
                html += `<div class="mt-3">
                <strong style="color: #4a9f9c;">مضمون التفويض:</strong>
                <ul class="list-group mt-2" style="border-radius: 10px; overflow: hidden;">`;
                nonBankSignaturesList.forEach(s => {
                    if (s.authorizationDetails || s.delegationContent) {
                        html += `<li class="list-group-item" style="border-color: #e0eceb; color: #2c5a58;">
                        <strong style="color: #4a9f9c;">${escapeHtml(s.name || s.fullName)}:</strong> 
                        ${escapeHtml(s.authorizationDetails || s.delegationContent || '-')}
                    </li>`;
                    }
                });
                html += `</ul></div>`;
            }
            html += `</div>`;
        } else {
            html += `<div class="mb-4" style="background: #ffffff; border-radius: 15px; padding: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.03);">
            <h5 style="color: #4a9f9c; border-right: 4px solid #7bc6c3; padding-right: 12px;">
                <i class="fas fa-file-signature me-2"></i>صلاحيات التوقيع غير البنكية
            </h5>
            <p class="text-muted" style="color: #7bc6c3;">لا توجد صلاحيات توقيع غير بنكية</p>
        </div>`;
        }

        return html;
    }
    // ========== EDIT COMPANY ==========
    window.editCompany = function (id) {
        ajaxRequest(API.getCompany + '?id=' + id, 'GET', null, function (company) {
            editingCompanyId = company.companyId;
            populateFormWithCompanyData(company);
            setTimeout(function () {
                forceRevalidateAllTabs();
            }, 500);
            $('#companyModalTitle').text('تعديل شركة');
            $('#companyModal').modal('show');
        });
    };

    function populateFormWithCompanyData(company) {
        $('#companyName').val(company.name || '');
        $('#mainAddress').val(company.headOfficeAddress || '');
        $('#commercialRecord').val(company.commercialRegNo || '');
        $('#registrationExpiryDate').val(formatDateForInput(company.registrationExpiry));
        $('#registrationExpiryNotificationDays').val(company.registrationNotificationPeriod || '');
        $('#commercialRecordDate').val(formatDateForInput(company.registrationDate));
        $('#taxCard').val(company.taxCardNumber || '');
        $('#taxCardRenewalDate').val(formatDateForInput(company.cardRenewalDate));
        $('#taxCardNotificationDays').val(company.cardRenewalDue || '');
        $('#vatNumber').val(company.vat || '');
        $('#auditor').val(company.accountingAuditor || '');
        $('#auditorAppointmentDate').val(formatDateForInput(company.accountingAuditorHiringDate));
        $('#authorizedCapital').val(company.authorizedCapital || '');
        $('#authorizedCapitalRecordDate').val(formatDateForInput(company.authorizedCapitalDate));
        $('#issuedCapital').val(company.issuedCapital || '');
        $('#issuedCapitalRecordDate').val(formatDateForInput(company.issuedCapitalDate));
        $('#paidCapital').val(company.paidUpCapital || '');
        $('#paidCapitalRecordDate').val(formatDateForInput(company.paidUpCapitalDate));
        $('#fixedShareValue').val(company.shareValue || 1.00);

        if (company.meetings) {
            if (company.meetings.ordinaryMeeting) {
                $('#ordinaryMeetingLocation').val(company.meetings.ordinaryMeeting.places || []);
                $('#ordinaryMeetingAddress').val(company.meetings.ordinaryMeeting.address || '');
                $('#ordinaryMeetingQuorum').val(company.meetings.ordinaryMeeting.validityValue || '');
                $('#ordinaryVotingMechanism').val(company.meetings.ordinaryMeeting.votingMechanism || '');
            }
            if (company.meetings.extraordinaryMeeting) {
                $('#extraordinaryMeetingLocation').val(company.meetings.extraordinaryMeeting.places || []);
                $('#extraordinaryMeetingAddress').val(company.meetings.extraordinaryMeeting.address || '');
                $('#extraordinaryMeetingQuorum').val(company.meetings.extraordinaryMeeting.validityValue || '');
                $('#extraordinaryVotingMechanism').val(company.meetings.extraordinaryMeeting.votingMechanism || '');
            }
            if (company.meetings.boardMeeting) {
                $('#boardMeetingLocation').val(company.meetings.boardMeeting.places || []);
                $('#boardMeetingAddress').val(company.meetings.boardMeeting.address || '');
                $('#boardMeetingQuorum').val(company.meetings.boardMeeting.validityValue || '');
                $('#boardVotingMechanism').val(company.meetings.boardMeeting.votingMechanism || '');
            }
        }

        if (company.boardSettings) {
            $('#boardDuration').val(company.boardSettings.duration || '');
            var startDateValue = '';
            if (company.boardSettings.startDate) {
                var cleanStartDate = company.boardSettings.startDate.split('T')[0];
                if (cleanStartDate && cleanStartDate.length >= 7) {
                    startDateValue = cleanStartDate.substring(0, 7);
                }
            }
            $('#boardStartDate').val(startDateValue);
            $('#boardExpiryNotificationDays').val(company.boardSettings.notificationPeriod || '');
        }

        shareholders = company.shareholders || [];
        renderShareholders();
        branches = company.branches || [];
        renderBranches();
        boardMembers = company.boardMembers || [];
        renderBoardMembers();
        // Populate Bank Signatures
        bankSignatures = [];
        if (company.bankSignatures && company.bankSignatures.length > 0) {
            company.bankSignatures.forEach(bg => {
                let membersArray = [];
                if (bg.members && Array.isArray(bg.members)) {
                    membersArray = bg.members;
                } else if (bg.members && typeof bg.members === 'string') {
                    membersArray = bg.members.split(',').map(m => m.trim());
                } else if (bg.authorizedPersons) {
                    membersArray = bg.authorizedPersons.split(',').map(m => m.trim());
                }

                bankSignatures.push({
                    groupId: bg.groupId || getNextId('nextBankSignatureId'),
                    groupName: bg.groupName || '',
                    members: membersArray,
                    note: bg.note || ''  
                });
            });
        }
        if (company.bankSignaturesNotes) {
            $('#bankSignaturesNotes').val(company.bankSignaturesNotes);
        }
        renderBankSignatures();
        nonBankSignatures = [];
        if (company.nonBankSignatures && company.nonBankSignatures.length > 0) {
            company.nonBankSignatures.forEach(nb => {
                nonBankSignatures.push({
                    authorizationId: nb.authorizationId || getNextId('nextNonBankSignatureId'),
                    name: nb.name || '',
                    issuedTo: nb.issuedTo || '',
                    authorizationDetails: nb.authorizationDetails || '',
                    expiryDate: nb.expiryDate,
                    note: '',  
                    notificationPerid: nb.notificationPerid || null
                });
            });
        }
        if (company.nonBankSignaturesNotes) {
            $('#nonBankSignaturesNotes').val(company.nonBankSignaturesNotes);  
        }
        renderNonBankSignatures();
    }

    // ========== DELETE COMPANY ==========
    window.deleteCompany = function (id) {
        if (confirm('هل أنت متأكد من حذف هذه الشركة؟')) {
            ajaxRequest(API.deleteCompany + '?id=' + id, 'DELETE', null, function (response) {
                showToast('تم حذف الشركة بنجاح', 'success');
                loadCompanies();
            });
        }
    };

    // ========== MANAGE ATTACHMENTS ==========
    window.manageAttachments = function (companyId) {
        window.location.href = `/Companies/ManageAttachments?id=${companyId}`;
    };
    // ========== SAVE COMPANY ==========
    function saveCompany() {
        // إعادة التحقق من جميع التابات
        validateCompanyInfo();
        validateShareholders();
        validateBranches();
        validateMeetings();
        validateBoardMembers();
        validateBankSignatures();
        validateNonBankSignatures();

        if (!validateAllTabs()) {
            if (!isCompanyInfoValid) showToast('يرجى إكمال بيانات الشركة أولاً', 'danger');
            else if (!isShareholdersValid) showToast('يرجى إكمال بيانات المساهمين أولاً', 'danger');
            else if (!isBranchesValid) showToast('يرجى إكمال بيانات الفروع أولاً', 'danger');
            else if (!isMeetingsValid) showToast('يرجى إكمال بيانات الاجتماعات أولاً', 'danger');
            else if (!isBoardMembersValid) showToast('يرجى إكمال بيانات أعضاء المجلس أولاً', 'danger');
            else if (!isBankSignaturesValid) showToast('يرجى إكمال بيانات الصلاحيات البنكية أولاً', 'danger');
            else if (!isNonBankSignaturesValid) showToast('يرجى إكمال بيانات الصلاحيات غير البنكية أولاً', 'danger');
            else showToast('يرجى إكمال جميع البيانات المطلوبة', 'danger');
            return;
        }
        const companyData = {
            companyId: editingCompanyId || 0,
            name: $('#companyName').val() || '',
            headOfficeAddress: $('#mainAddress').val() || '',
            commercialRegNo: $('#commercialRecord').val() || '',
            registrationExpiry: formatDateForSave($('#registrationExpiryDate').val()),
            registrationNotificationPeriod: parseInt($('#registrationExpiryNotificationDays').val()) || null,
            registrationDate: formatDateForSave($('#commercialRecordDate').val()),
            taxCardNumber: $('#taxCard').val() || '',
            cardRenewalDate: formatDateForSave($('#taxCardRenewalDate').val()),
            cardRenewalDue: parseInt($('#taxCardNotificationDays').val()) || null,
            vat: parseFloat($('#vatNumber').val()) || null,
            accountingAuditor: $('#auditor').val() || '',
            accountingAuditorHiringDate: formatDateForSave($('#auditorAppointmentDate').val()),
            authorizedCapital: parseFloat($('#authorizedCapital').val()) || null,
            authorizedCapitalDate: formatDateForSave($('#authorizedCapitalRecordDate').val()),
            issuedCapital: parseFloat($('#issuedCapital').val()) || null,
            issuedCapitalDate: formatDateForSave($('#issuedCapitalRecordDate').val()),
            paidUpCapital: parseFloat($('#paidCapital').val()) || null,
            paidUpCapitalDate: formatDateForSave($('#paidCapitalRecordDate').val()),
            shareValue: parseFloat($('#fixedShareValue').val()) || 1.00,
            meetings: {
                ordinaryMeeting: $('#ordinaryMeetingLocation').val() && $('#ordinaryMeetingLocation').val().length > 0 ? {
                    associationType: 'ordinary',
                    places: $('#ordinaryMeetingLocation').val() || [],
                    address: $('#ordinaryMeetingAddress').val() || '',
                    validityValue: parseFloat($('#ordinaryMeetingQuorum').val()) || null,
                    votingMechanism: $('#ordinaryVotingMechanism').val() || ''
                } : null,
                extraordinaryMeeting: $('#extraordinaryMeetingLocation').val() && $('#extraordinaryMeetingLocation').val().length > 0 ? {
                    associationType: 'extraordinary',
                    places: $('#extraordinaryMeetingLocation').val() || [],
                    address: $('#extraordinaryMeetingAddress').val() || '',
                    validityValue: parseFloat($('#extraordinaryMeetingQuorum').val()) || null,
                    votingMechanism: $('#extraordinaryVotingMechanism').val() || ''
                } : null,
                boardMeeting: $('#boardMeetingLocation').val() && $('#boardMeetingLocation').val().length > 0 ? {
                    associationType: 'board',
                    places: $('#boardMeetingLocation').val() || [],
                    address: $('#boardMeetingAddress').val() || '',
                    validityValue: parseFloat($('#boardMeetingQuorum').val()) || null,
                    votingMechanism: $('#boardVotingMechanism').val() || ''
                } : null
            },
            shareholders: shareholders.map(s => ({
                shareholderId: (s.shareholderId && s.shareholderId > 0) ? s.shareholderId : 0,
                name: s.name || '',
                role: s.role || '',
                sharesPercentage: s.sharesPercentage || '0',
                founderName: s.founderName || '',
                founderShareCount: s.founderShareCount || 0,
                subscribedShareCount: s.subscribedShareCount || 0,
                excellentShareCount: s.excellentShareCount || 0,
                startDate: s.startDate ? formatDateForSave(s.startDate) : null,
                endDate: s.endDate ? formatDateForSave(s.endDate) : null,
                totalShareCounts: s.totalShareCounts || 0
            })),
            branches: branches.map(b => ({
                branchId: (b.branchId && b.branchId > 0) ? b.branchId : 0,
                name: b.name || '',
                registrationNumber: b.registrationNumber || '',
                expiryDate: b.expiryDate ? formatDateForSave(b.expiryDate) : null,
                address: b.address || '',
                registrationNotification: b.registrationNotification || null
            })),
            boardSettings: {
                boardId: 0,
                duration: $('#boardDuration').val() || '',
                startDate: $('#boardStartDate').val() ? $('#boardStartDate').val() + '-01' : null,
                notificationPeriod: parseInt($('#boardExpiryNotificationDays').val()) || null
            },
            boardMembers: boardMembers.map(m => ({
                memberId: (m.memberId && m.memberId > 0) ? m.memberId : 0,
                name: m.name || '',
                position: Array.isArray(m.position) ? m.position.join(',') : (m.position || '')
            })),
            bankSignatures: bankSignatures.map(bg => ({
                groupId: (bg.groupId && bg.groupId > 0) ? bg.groupId : 0,
                groupName: bg.groupName || '',
                members: bg.members || [],
                note: ''  
            })),
            bankSignaturesNotes: $('#bankSignaturesNotes').val() || '',  
            nonBankSignatures: nonBankSignatures.map(nb => ({
                authorizationId: (nb.authorizationId && nb.authorizationId > 0) ? nb.authorizationId : 0,
                name: nb.name || '',
                issuedTo: nb.issuedTo || '',
                authorizationDetails: nb.authorizationDetails || '',
                expiryDate: nb.expiryDate ? formatDateForSave(nb.expiryDate) : null,
                note: '',  
                notificationPerid: nb.notificationPerid || null
            })),
            nonBankSignaturesNotes: $('#nonBankSignaturesNotes').val() || '', 
        };
      
        $.ajax({
            url: API.saveCompany,
            type: 'POST',
            contentType: 'application/json',
            headers: { 'RequestVerificationToken': window.antiForgeryToken },
            data: JSON.stringify(companyData),
            success: function (response) {
                if (response.success) {
                    showToast('تم حفظ الشركة بنجاح', 'success');
                    $('#companyModal').modal('hide');
                    loadCompanies();
                    resetAllForms();
                } else {
                    showToast(response.message || 'حدث خطأ أثناء حفظ الشركة', 'danger');
                }
            },
            error: function (xhr, status, error) {
                console.error('Save error:', { xhr, status, error });
                let errorMsg = 'حدث خطأ أثناء حفظ الشركة';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMsg = xhr.responseJSON.message;
                    if (xhr.responseJSON.errors) {
                        console.error('Validation errors:', xhr.responseJSON.errors);
                    }
                } else if (xhr.status === 400) {
                    errorMsg = 'بيانات غير صحيحة - تأكد من جميع الحقول';
                } else if (xhr.status === 500) {
                    errorMsg = 'خطأ في الخادم - راجع السجلات';
                }
                showToast(errorMsg, 'danger');
            }
        });
    }

    // ========== RENDER FUNCTIONS ==========

    // ========== SHAREHOLDERS MANAGEMENT ==========
    function renderShareholders() {
        if (shareholders.length === 0) {
            $('#shareholdersList').html(`
            <div class="text-center text-muted py-4">
                <i class="fas fa-users fa-2x mb-2 d-block"></i>
                لا يوجد مساهمين حالياً
                <div class="invalid-feedback d-block mt-2" style="display: block;">يجب إضافة مساهم واحد على الأقل</div>
            </div>
        `);
            validateShareholders();
            return;
        }

        const activeShareholders = shareholders.filter(s => !s.endDate);
        const exitedShareholders = shareholders.filter(s => s.endDate);
        const shareValue = parseFloat($('#fixedShareValue').val()) || 1.00;

        let html = '';

        if (activeShareholders.length > 0) {
            html += `<div class="mb-4">
            <h6 class="text-success mb-3"><i class="fas fa-user-check me-2"></i>المساهمين النشطين (${activeShareholders.length})</h6>
            <div class="shareholders-grid">`;

            activeShareholders.forEach((s) => {
                html += `
                <div class="shareholder-card card mb-3 border-success" data-id="${s.shareholderId}">
                    <div class="card-header d-flex justify-content-between align-items-center" style="cursor: pointer;" onclick="toggleShareholderExpand(${s.shareholderId})">
                        <div>
                            <i class="fas fa-user-circle me-2"></i>
                            <strong>${escapeHtml(s.name)}</strong>
                            <span class="badge bg-teal ms-2">${s.role || '-'}</span>
                            <span class="badge bg-info ms-2">${s.sharesPercentage || '0'}%</span>
                            <span class="badge bg-success ms-2"><i class="fas fa-calendar-alt"></i> دخل: ${formatDate(s.startDate)}</span>
                        </div>
                        <div>
                            <button class="btn btn-sm btn-warning me-1" onclick="editShareholder(${s.shareholderId}, event)">تعديل</button>
                            <button class="btn btn-sm btn-danger me-1" onclick="exitShareholder(${s.shareholderId}, event)">تسجيل خروج</button>
                            <button class="btn btn-sm btn-danger" onclick="deleteShareholder(${s.shareholderId}, event)">حذف</button>
                            <i class="fas fa-chevron-down ms-2 expand-icon"></i>
                        </div>
                    </div>
                    <div class="card-body shareholder-details" id="shareholder-details-${s.shareholderId}" style="display: none;">
                        <div class="row">
                            <div class="col-md-3"><small class="text-muted">اسم المؤسس:</small><div>${escapeHtml(s.founderName || '-')}</div></div>
                            <div class="col-md-3"><small class="text-muted">أسهم مكتتبة:</small><div>${s.subscribedShareCount || 0}</div></div>
                            <div class="col-md-3"><small class="text-muted">أسهم ممتازة:</small><div>${s.excellentShareCount || 0}</div></div>
                            <div class="col-md-3"><small class="text-muted">أسهم مؤسس:</small><div>${s.founderShareCount || 0}</div></div>
                            <div class="col-md-3"><small class="text-muted">قيمة السهم:</small><div>${shareValue} جنيه</div></div>
                        </div>
                    </div>
                </div>
            `;
            });
            html += `</div></div>`;
        }

        if (exitedShareholders.length > 0) {
            html += `<div class="mt-4">
            <h6 class="text-danger mb-3"><i class="fas fa-user-slash me-2"></i>المساهمين السابقين (${exitedShareholders.length})</h6>
            <div class="shareholders-grid">`;

            exitedShareholders.forEach((s) => {
                html += `
                <div class="shareholder-card card mb-3 border-danger" data-id="${s.shareholderId}">
                    <div class="card-header d-flex justify-content-between align-items-center" style="cursor: pointer;" onclick="toggleShareholderExpand(${s.shareholderId})">
                        <div>
                            <i class="fas fa-user-circle me-2"></i>
                            <strong>${escapeHtml(s.name)}</strong>
                            <span class="badge bg-teal ms-2">${s.role || '-'}</span>
                            <span class="badge bg-secondary ms-2">${s.sharesPercentage || '0'}%</span>
                            <span class="badge bg-success ms-2"><i class="fas fa-calendar-alt"></i> دخل: ${formatDate(s.startDate)}</span>
                            <span class="badge bg-danger ms-2"><i class="fas fa-sign-out-alt"></i> خرج: ${formatDate(s.endDate)}</span>
                        </div>
                        <div>
                            <button class="btn btn-sm btn-warning me-1" onclick="editShareholder(${s.shareholderId}, event)">تعديل</button>
                            <button class="btn btn-sm btn-info me-1" onclick="restoreShareholder(${s.shareholderId}, event)">إعادة تفعيل</button>
                            <button class="btn btn-sm btn-danger" onclick="deleteShareholder(${s.shareholderId}, event)">حذف</button>
                            <i class="fas fa-chevron-down ms-2 expand-icon"></i>
                        </div>
                    </div>
                    <div class="card-body shareholder-details" id="shareholder-details-${s.shareholderId}" style="display: none;">
                        <div class="row">
                            <div class="col-md-3"><small class="text-muted">اسم المؤسس:</small><div>${escapeHtml(s.founderName || '-')}</div></div>
                            <div class="col-md-3"><small class="text-muted">أسهم مكتتبة:</small><div>${s.subscribedShareCount || 0}</div></div>
                            <div class="col-md-3"><small class="text-muted">أسهم ممتازة:</small><div>${s.excellentShareCount || 0}</div></div>
                            <div class="col-md-3"><small class="text-muted">أسهم مؤسس:</small><div>${s.founderShareCount || 0}</div></div>
                            <div class="col-md-3"><small class="text-muted">قيمة السهم:</small><div>${shareValue} جنيه</div></div>
                        </div>
                    </div>
                </div>
            `;
            });
            html += `</div></div>`;
        }

        $('#shareholdersList').html(html);
        validateShareholders();
    }

    window.toggleShareholderExpand = function (id) {
        $(`#shareholder-details-${id}`).slideToggle();
        $(`.shareholder-card[data-id="${id}"] .expand-icon`).toggleClass('fa-chevron-down fa-chevron-up');
    };

    window.editShareholder = function (id, event) {
        event.stopPropagation();
        const shareholder = shareholders.find(s => s.shareholderId === id);
        if (shareholder) {
            $('#shareholderModalTitle').text('تعديل مساهم');
            $('#shareholderId').val(shareholder.shareholderId);
            $('#shareholderName').val(shareholder.name);
            $('#shareholderTitle').val(shareholder.role);
            $('#founderName').val(shareholder.founderName || '');
            $('#subscribedShares').val(shareholder.subscribedShareCount || 0);
            $('#preferredShares').val(shareholder.excellentShareCount || 0);
            $('#founderShares').val(shareholder.founderShareCount || 0);
            $('#entryDate').val(formatDateForInput(shareholder.startDate));
            $('#exitDate').val(formatDateForInput(shareholder.endDate));
            $('#shareholderModal').modal('show');
        }
    };

    window.deleteShareholder = function (id, event) {
        event.stopPropagation();
        if (confirm('هل أنت متأكد من حذف هذا المساهم نهائياً؟')) {
            shareholders = shareholders.filter(s => s.shareholderId !== id);
            renderShareholders();
            showToast('تم حذف المساهم بنجاح', 'success');
        }
    };

    window.exitShareholder = function (id, event) {
        event.stopPropagation();
        const shareholder = shareholders.find(s => s.shareholderId === id);
        if (shareholder && !shareholder.endDate) {
            $('#exitShareholderName').text(shareholder.name);
            $('#confirmExitDate').val(new Date().toISOString().split('T')[0]);
            $('#confirmExitModal').data('exit-id', id).modal('show');
        }
    };

    window.restoreShareholder = function (id, event) {
        event.stopPropagation();
        const shareholder = shareholders.find(s => s.shareholderId === id);
        if (shareholder && shareholder.endDate) {
            if (confirm(`هل أنت متأكد من إعادة تفعيل المساهم "${shareholder.name}"؟`)) {
                shareholder.endDate = null;
                renderShareholders();
                showToast('تم إعادة تفعيل المساهم بنجاح', 'success');
            }
        }
    };

    $('#addShareholderBtn').click(function () {
        $('#shareholderModalTitle').text('إضافة مساهم');
        $('#shareholderForm')[0].reset();
        $('#shareholderId').val(getNextId('nextShareholderId'));
        $('#shareholderTitle').val('مؤسس');
        $('#subscribedShares').val(0);
        $('#preferredShares').val(0);
        $('#founderShares').val(0);
        $('#entryDate').val(new Date().toISOString().split('T')[0]);
        $('#exitDate').val('');
        calculateShareholderValues();
        $('#shareholderModal').modal('show');
    });

    $('#saveShareholderBtn').click(function () {
        if (!$('#shareholderName').val()) {
            showToast('يرجى إدخال اسم المساهم', 'danger');
            return;
        }
        if (!$('#entryDate').val()) {
            showToast('يرجى إدخال تاريخ الدخول', 'danger');
            return;
        }

        const shareValue = parseFloat($('#fixedShareValue').val()) || 1.00;
        const subscribed = parseFloat($('#subscribedShares').val()) || 0;
        const preferred = parseFloat($('#preferredShares').val()) || 0;
        const founder = parseFloat($('#founderShares').val()) || 0;
        const totalShares = subscribed + preferred + founder;
        const totalValue = totalShares * shareValue;

        const authorizedCapital = parseFloat($('#authorizedCapital').val()) || 0;
        const issuedCapital = parseFloat($('#issuedCapital').val()) || 0;
        const paidCapital = parseFloat($('#paidCapital').val()) || 0;
        const totalCompanyCapital = authorizedCapital || issuedCapital || paidCapital || 1;

        const percentage = (totalValue / totalCompanyCapital) * 100;

        const shareholderId = $('#shareholderId').val();

        const shareholderData = {
            shareholderId: shareholderId ? parseInt(shareholderId) : getNextId('nextShareholderId'),
            name: $('#shareholderName').val(),
            role: $('#shareholderTitle').val(),
            sharesPercentage: percentage.toFixed(2),
            founderName: $('#founderName').val(),
            founderShareCount: founder,
            subscribedShareCount: subscribed,
            excellentShareCount: preferred,
            startDate: $('#entryDate').val(),
            endDate: $('#exitDate').val() || null,
            totalShareCounts: totalShares
        };

        const existingIndex = shareholders.findIndex(s => s.shareholderId === shareholderData.shareholderId);
        if (existingIndex !== -1) {
            shareholders[existingIndex] = shareholderData;
        } else {
            shareholders.push(shareholderData);
        }

        $('#shareholderModal').modal('hide');
        renderShareholders();
        showToast('تم حفظ المساهم بنجاح', 'success');
    });

    $('#confirmExitBtn').click(function () {
        const exitDate = $('#confirmExitDate').val();
        const exitId = $('#confirmExitModal').data('exit-id');
        if (!exitDate || !exitId) return;

        const shareholder = shareholders.find(s => s.shareholderId === exitId);
        if (shareholder) {
            shareholder.endDate = exitDate;
            renderShareholders();
            showToast(`تم تسجيل خروج المساهم "${shareholder.name}"`, 'success');
        }
        $('#confirmExitModal').modal('hide');
    });

    function calculateShareholderValues() {
        const subscribed = parseFloat($('#subscribedShares').val()) || 0;
        const preferred = parseFloat($('#preferredShares').val()) || 0;
        const founder = parseFloat($('#founderShares').val()) || 0;
        const shareValue = parseFloat($('#fixedShareValue').val()) || 1.00;
        const totalValue = (subscribed + preferred + founder) * shareValue;
        $('#totalSharesValue').val(totalValue.toFixed(2));

        const authorizedCapital = parseFloat($('#authorizedCapital').val()) || 0;
        const issuedCapital = parseFloat($('#issuedCapital').val()) || 0;
        const paidCapital = parseFloat($('#paidCapital').val()) || 0;
        const totalCompanyCapital = authorizedCapital || issuedCapital || paidCapital || 1;

        const percentage = (totalValue / totalCompanyCapital) * 100;
        $('#sharePercentage').val(percentage.toFixed(2));
    }

    // ========== BRANCHES MANAGEMENT ==========
    function renderBranches() {
        if (branches.length === 0) {
            $('#branchesList').html(`
            <div class="text-center text-muted py-4">
                <i class="fas fa-code-branch fa-2x mb-2 d-block"></i>
                لا يوجد فروع حالياً
                <div class="invalid-feedback d-block mt-2" style="display: block;">يجب إضافة فرع واحد على الأقل</div>
            </div>
        `);
            validateBranches();
            return;
        }

        let html = '<div class="branches-grid">';
        branches.forEach((b) => {
            html += `
            <div class="branch-card card mb-3" data-id="${b.branchId}">
                <div class="card-header d-flex justify-content-between align-items-center" style="cursor: pointer;" onclick="toggleBranchExpand(${b.branchId})">
                    <div>
                        <i class="fas fa-store me-2"></i>
                        <strong>${escapeHtml(b.name)}</strong>
                        <span class="badge bg-teal ms-2">${b.registrationNumber || 'بدون سجل'}</span>
                    </div>
                    <div>
                        <button class="btn btn-sm btn-warning me-1" onclick="editBranch(${b.branchId}, event)">تعديل</button>
                        <button class="btn btn-sm btn-danger" onclick="deleteBranch(${b.branchId}, event)">حذف</button>
                        <i class="fas fa-chevron-down ms-2 expand-icon"></i>
                    </div>
                </div>
                <div class="card-body branch-details" id="branch-details-${b.branchId}" style="display: none;">
                    <div class="row">
                        <div class="col-md-4"><small class="text-muted">عنوان الفرع:</small><div>${escapeHtml(b.address || '-')}</div></div>
                        <div class="col-md-4"><small class="text-muted">رقم السجل:</small><div>${escapeHtml(b.registrationNumber || '-')}</div></div>
                        <div class="col-md-4"><small class="text-muted">تاريخ انتهاء سريان القيد:</small><div>${formatDate(b.expiryDate) || '-'}</div></div>
                        <div class="col-md-4"><small class="text-muted">إشعار قبل انتهاء القيد (أيام):</small><div>${b.registrationNotification ? b.registrationNotification + ' يوم' : '-'}</div></div>
                    </div>
                </div>
            </div>
        `;
        });
        html += '</div>';
        $('#branchesList').html(html);
        validateBranches();
    }

    window.toggleBranchExpand = function (id) {
        $(`#branch-details-${id}`).slideToggle();
        $(`.branch-card[data-id="${id}"] .expand-icon`).toggleClass('fa-chevron-down fa-chevron-up');
    };

    window.editBranch = function (id, event) {
        event.stopPropagation();
        const branch = branches.find(b => b.branchId === id);
        if (branch) {
            $('#branchModalTitle').text('تعديل فرع');
            $('#branchId').val(branch.branchId);
            $('#branchName').val(branch.name);
            $('#branchAddress').val(branch.address || '');
            $('#branchRecord').val(branch.registrationNumber || '');
            $('#branchEffectiveDate').val(formatDateForInput(branch.expiryDate));
            $('#branchExpiryNotificationDays').val(branch.registrationNotification || '');
            $('#branchModal').modal('show');
        }
    };

    window.deleteBranch = function (id, event) {
        event.stopPropagation();
        if (confirm('هل أنت متأكد من حذف هذا الفرع؟')) {
            branches = branches.filter(b => b.branchId !== id);
            renderBranches();
            showToast('تم حذف الفرع بنجاح', 'success');
        }
    };

    $('#addBranchBtn').click(function () {
        $('#branchModalTitle').text('إضافة فرع');
        $('#branchForm')[0].reset();
        $('#branchId').val(getNextId('nextBranchId'));
        $('#branchModal').modal('show');
    });

    $('#saveBranchBtn').click(function () {
        if (!$('#branchName').val()) {
            showToast('يرجى إدخال اسم الفرع', 'danger');
            return;
        }
        if (!$('#branchAddress').val()) {
            showToast('يرجى إدخال عنوان الفرع', 'danger');
            return;
        }

        const branchId = $('#branchId').val();

        const branchData = {
            branchId: branchId ? parseInt(branchId) : getNextId('nextBranchId'),
            name: $('#branchName').val(),
            registrationNumber: $('#branchRecord').val(),
            expiryDate: formatDateForSave($('#branchEffectiveDate').val()),
            address: $('#branchAddress').val(),
            registrationNotification: parseInt($('#branchExpiryNotificationDays').val()) || null
        };

        const existingIndex = branches.findIndex(b => b.branchId === branchData.branchId);
        if (existingIndex !== -1) {
            branches[existingIndex] = branchData;
        } else {
            branches.push(branchData);
        }

        $('#branchModal').modal('hide');
        renderBranches();
        showToast('تم حفظ الفرع بنجاح', 'success');
    });

    // ========== BOARD MEMBERS MANAGEMENT ==========
    function renderBoardMembers() {
        if (boardMembers.length === 0) {
            $('#boardMembersList').html(`
            <div class="text-center text-muted py-4">
                <i class="fas fa-user-tie fa-2x mb-2 d-block"></i>
                لا يوجد أعضاء مجلس إدارة حالياً
                <div class="invalid-feedback d-block mt-2" style="display: block;">يجب إضافة عضو واحد على الأقل</div>
            </div>
        `);
            validateBoardMembers();
            return;
        }

        let html = '<div class="board-members-grid">';
        boardMembers.forEach((m) => {
            html += `
            <div class="board-member-card card mb-3" data-id="${m.memberId}">
                <div class="card-header d-flex justify-content-between align-items-center" style="cursor: pointer;" onclick="toggleBoardMemberExpand(${m.memberId})">
                    <div>
                        <i class="fas fa-user-tie me-2"></i>
                        <strong>${escapeHtml(m.name)}</strong>
                        <span class="badge bg-teal ms-2">${escapeHtml(m.position)}</span>
                    </div>
                    <div>
                        <button class="btn btn-sm btn-warning me-1" onclick="editBoardMember(${m.memberId}, event)">تعديل</button>
                        <button class="btn btn-sm btn-danger" onclick="deleteBoardMember(${m.memberId}, event)">حذف</button>
                        <i class="fas fa-chevron-down ms-2 expand-icon"></i>
                    </div>
                </div>
                <div class="card-body board-member-details" id="board-member-details-${m.memberId}" style="display: none;">
                    <div class="row">
                        <div class="col-md-6"><small class="text-muted">اسم العضو:</small><div>${escapeHtml(m.name)}</div></div>
                        <div class="col-md-6"><small class="text-muted">الصفة:</small><div>${escapeHtml(m.position)}</div></div>
                    </div>
                </div>
            </div>
        `;
        });
        html += '</div>';
        $('#boardMembersList').html(html);
        validateBoardMembers();
    }

    window.toggleBoardMemberExpand = function (id) {
        $(`#board-member-details-${id}`).slideToggle();
        $(`.board-member-card[data-id="${id}"] .expand-icon`).toggleClass('fa-chevron-down fa-chevron-up');
    };

    window.editBoardMember = function (id, event) {
        event.stopPropagation();
        const member = boardMembers.find(m => m.memberId === id);
        if (member) {
            $('#boardMemberModalTitle').text('تعديل عضو مجلس إدارة');
            $('#boardMemberId').val(member.memberId);
            $('#boardMemberName').val(member.name);
            $('#boardMemberTitle').val(member.position.split(','));
            $('#boardMemberModal').modal('show');
        }
    };

    window.deleteBoardMember = function (id, event) {
        event.stopPropagation();
        if (confirm('هل أنت متأكد من حذف هذا العضو؟')) {
            boardMembers = boardMembers.filter(m => m.memberId !== id);
            renderBoardMembers();
            showToast('تم حذف العضو بنجاح', 'success');
        }
    };

    $('#addBoardMemberBtn').click(function () {
        $('#boardMemberModalTitle').text('إضافة عضو مجلس إدارة');
        $('#boardMemberForm')[0].reset();
        $('#boardMemberId').val(getNextId('nextBoardMemberId'));
        $('#boardMemberTitle').val([]);
        $('#boardMemberModal').modal('show');
    });

    $('#saveBoardMemberBtn').click(function () {
        if (!$('#boardMemberName').val()) {
            showToast('يرجى إدخال اسم العضو', 'danger');
            return;
        }

        const selectedTitles = $('#boardMemberTitle').val();
        if (!selectedTitles || selectedTitles.length === 0) {
            showToast('يرجى اختيار صفة واحدة على الأقل للعضو', 'danger');
            return;
        }

        const memberId = $('#boardMemberId').val();

        const memberData = {
            memberId: memberId ? parseInt(memberId) : getNextId('nextBoardMemberId'),
            name: $('#boardMemberName').val(),
            position: selectedTitles.join(',')
        };

        const existingIndex = boardMembers.findIndex(m => m.memberId === memberData.memberId);
        if (existingIndex !== -1) {
            boardMembers[existingIndex] = memberData;
        } else {
            boardMembers.push(memberData);
        }

        $('#boardMemberModal').modal('hide');
        renderBoardMembers();
        showToast('تم حفظ العضو بنجاح', 'success');
    });

    // ========== BANK SIGNATURES MANAGEMENT ==========
    function renderBankSignatures() {
        if (bankSignatures.length === 0) {
            $('#bankSignaturesList').html(`
            <div class="text-center text-muted py-4">
                <i class="fas fa-university fa-2x mb-2 d-block"></i>
                لا يوجد مجموعات توقيع بنكية مضافة حالياً
                <div class="invalid-feedback d-block mt-2" style="display: block;">يجب إضافة مجموعة توقيع بنكية واحدة على الأقل</div>
            </div>
        `);
            validateBankSignatures();
            return;
        }

        let html = '<div class="bank-signatures-grid">';
        bankSignatures.forEach((bg) => {
            // عرض الأسماء بشكل منفصل
            let membersList = '';
            if (bg.members && bg.members.length > 0) {
                membersList = '<ul class="list-unstyled mb-0" style="margin: 0; padding-right: 15px;">';
                bg.members.forEach(member => {
                    membersList += `<li style="margin-bottom: 3px;">• ${escapeHtml(member)}</li>`;
                });
                membersList += '</ul>';
            }

            html += `
            <div class="bank-signature-card card mb-3" data-id="${bg.groupId}">
                <div class="card-header d-flex justify-content-between align-items-center" style="cursor: pointer;" onclick="toggleBankSignatureExpand(${bg.groupId})">
                    <div>
                        <i class="fas fa-users me-2"></i>
                        <strong>${escapeHtml(bg.groupName)}</strong>
                        <span class="badge bg-teal ms-2">${bg.members.length} أشخاص</span>
                        ${bg.note ? `<span class="badge bg-info ms-2"><i class="fas fa-sticky-note"></i> ملاحظة</span>` : ''}
                    </div>
                    <div>
                        <button class="btn btn-sm btn-danger" onclick="deleteBankSignature(${bg.groupId}, event)">حذف</button>
                        <i class="fas fa-chevron-down ms-2 expand-icon"></i>
                    </div>
                </div>
                <div class="card-body bank-signature-details" id="bank-signature-details-${bg.groupId}" style="display: none;">
                    <div class="row">
                        <div class="col-md-12">
                            <small class="text-muted">الأشخاص المصرح لهم:</small>
                            <div class="mt-1">${membersList || '-'}</div>
                        </div>
                        ${bg.note ? `
                        <div class="col-md-12 mt-2">
                            <small class="text-muted">الملاحظات:</small>
                            <div class="mt-1">${escapeHtml(bg.note)}</div>
                        </div>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
        });
        html += '</div>';
        $('#bankSignaturesList').html(html);
        validateBankSignatures();
    }
    window.toggleBankSignatureExpand = function (id) {
        $(`#bank-signature-details-${id}`).slideToggle();
        $(`.bank-signature-card[data-id="${id}"] .expand-icon`).toggleClass('fa-chevron-down fa-chevron-up');
    };

    window.deleteBankSignature = function (id, event) {
        event.stopPropagation();
        if (confirm('هل أنت متأكد من حذف هذه المجموعة؟')) {
            bankSignatures = bankSignatures.filter(bg => bg.groupId !== id);
            renderBankSignatures();
            showToast('تم حذف المجموعة بنجاح', 'success');
        }
    };

$('#addBankSignatureBtn').click(function () {
    const groupName = $('#bankSignatureGroupSelect').val();
    const authorizedPersons = $('#authorizedPersons').val();

    if (!groupName) {
        showToast('يرجى اختيار مجموعة التوقيع', 'danger');
        return;
    }
    if (!authorizedPersons) {
        showToast('يرجى إدخال الأشخاص المصرح لهم', 'danger');
        return;
    }
    if (bankSignatures.some(bg => bg.groupName === groupName)) {
        showToast('هذه المجموعة مضافة بالفعل', 'danger');
        return;
    }

    let membersArray = authorizedPersons.split(',').map(p => p.trim()).filter(p => p !== '');
    
    if (membersArray.length === 0) {
        showToast('يرجى إدخال اسم شخص واحد على الأقل', 'danger');
        return;
    }


    bankSignatures.push({
        groupId: getNextId('nextBankSignatureId'),
        groupName: groupName,
        members: membersArray,  
        note: ''
    });

    $('#bankSignatureGroupSelect').val('');
    $('#authorizedPersons').val('');
    renderBankSignatures();
    showToast(`تم إضافة المجموعة بنجاح (${membersArray.length} شخص)`, 'success');
});
    // ========== NON-BANK SIGNATURES MANAGEMENT ==========
    function renderNonBankSignatures() {
        if (nonBankSignatures.length === 0) {
            $('#nonBankSignaturesList').html(`
            <div class="text-center text-muted py-4">
                <i class="fas fa-file-signature fa-2x mb-2 d-block"></i>
                لا يوجد تفويضات توقيع غير بنكية حالياً
                <div class="invalid-feedback d-block mt-2" style="display: block;">يجب إضافة تفويض توقيع غير بنكي واحد على الأقل</div>
            </div>
        `);
            validateNonBankSignatures();
            return;
        }

        let html = '<div class="non-bank-signatures-grid">';
        nonBankSignatures.forEach((nb) => {
            html += `
            <div class="non-bank-signature-card card mb-3" data-id="${nb.authorizationId}">
                <div class="card-header d-flex justify-content-between align-items-center" style="cursor: pointer;" onclick="toggleNonBankSignatureExpand(${nb.authorizationId})">
                    <div>
                        <i class="fas fa-user me-2"></i>
                        <strong>${escapeHtml(nb.name)}</strong>
                        <span class="badge bg-teal ms-2">${formatDate(nb.expiryDate) || 'بدون تاريخ'}</span>
                    </div>
                    <div>
                        <button class="btn btn-sm btn-warning me-1" onclick="editNonBankSignature(${nb.authorizationId}, event)">تعديل</button>
                        <button class="btn btn-sm btn-danger" onclick="deleteNonBankSignature(${nb.authorizationId}, event)">حذف</button>
                        <i class="fas fa-chevron-down ms-2 expand-icon"></i>
                    </div>
                </div>
                <div class="card-body non-bank-signature-details" id="non-bank-signature-details-${nb.authorizationId}" style="display: none;">
                    <div class="row">
                        <div class="col-md-6"><small class="text-muted">صادر لصالح:</small><div>${escapeHtml(nb.issuedTo || '-')}</div></div>
                        <div class="col-md-6"><small class="text-muted">تاريخ الانتهاء:</small><div>${formatDate(nb.expiryDate) || '-'}</div></div>
                        <div class="col-md-6"><small class="text-muted">إشعار قبل الانتهاء:</small><div>${nb.notificationPerid ? nb.notificationPerid + ' يوم' : '-'}</div></div>
                        <div class="col-md-12 mt-2">
                            <small class="text-muted">مضمون التفويض:</small>
                            <div class="mt-1">${escapeHtml(nb.authorizationDetails || '-')}</div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        });
        html += '</div>';
        $('#nonBankSignaturesList').html(html);
        validateNonBankSignatures();
    }

    window.toggleNonBankSignatureExpand = function (id) {
        $(`#non-bank-signature-details-${id}`).slideToggle();
        $(`.non-bank-signature-card[data-id="${id}"] .expand-icon`).toggleClass('fa-chevron-down fa-chevron-up');
    };

    window.editNonBankSignature = function (id, event) {
        event.stopPropagation();
        const signature = nonBankSignatures.find(nb => nb.authorizationId === id);
        if (signature) {
            $('#nonBankSignatureModalTitle').text('تعديل تفويض توقيع غير بنكي');
            $('#nonBankSignatureId').val(signature.authorizationId);
            $('#fullName').val(signature.name);
            $('#issuedToNonBank').val(signature.issuedTo);
            $('#delegationContentNonBank').val(signature.authorizationDetails);
            $('#expiryDate').val(formatDateForInput(signature.expiryDate));
            $('#expiryNotificationDays').val(signature.notificationPerid || '');
            $('#nonBankSignatureModal').modal('show');
        }
    };

    window.deleteNonBankSignature = function (id, event) {
        event.stopPropagation();
        if (confirm('هل أنت متأكد من حذف هذا التفويض؟')) {
            nonBankSignatures = nonBankSignatures.filter(nb => nb.authorizationId !== id);
            renderNonBankSignatures();
            showToast('تم حذف التفويض بنجاح', 'success');
        }
    };
    $('#addNonBankSignatureBtn').click(function () {
        $('#nonBankSignatureModalTitle').text('إضافة تفويض توقيع غير بنكي');
        $('#nonBankSignatureForm')[0].reset();
        $('#nonBankSignatureId').val(getNextId('nextNonBankSignatureId'));
        $('#nonBankSignatureModal').modal('show');
    });
    $('#saveNonBankSignatureBtn').click(function () {
        let isValid = true;
        if (!$('#fullName').val()) { $('#fullName').addClass('is-invalid'); isValid = false; }
        if (!$('#issuedToNonBank').val()) { $('#issuedToNonBank').addClass('is-invalid'); isValid = false; }
        if (!$('#delegationContentNonBank').val()) { $('#delegationContentNonBank').addClass('is-invalid'); isValid = false; }
        if (!$('#expiryDate').val()) { $('#expiryDate').addClass('is-invalid'); isValid = false; }

        if (!isValid) {
            showToast('يرجى إدخال جميع البيانات المطلوبة', 'danger');
            return;
        }

        const signatureId = $('#nonBankSignatureId').val();
        const signatureData = {
            authorizationId: signatureId ? parseInt(signatureId) : getNextId('nextNonBankSignatureId'),
            name: $('#fullName').val(),
            issuedTo: $('#issuedToNonBank').val(),
            authorizationDetails: $('#delegationContentNonBank').val(),
            expiryDate: formatDateForSave($('#expiryDate').val()),
            note: '', 
            notificationPerid: parseInt($('#expiryNotificationDays').val()) || null
        };

        const existingIndex = nonBankSignatures.findIndex(nb => nb.authorizationId === signatureData.authorizationId);
        if (existingIndex !== -1) {
            nonBankSignatures[existingIndex] = signatureData;
        } else {
            nonBankSignatures.push(signatureData);
        }

        $('#nonBankSignatureModal').modal('hide');
        renderNonBankSignatures();
        showToast('تم حفظ التفويض بنجاح', 'success');
    });
   
    function validateCompanyInfo() {
        let missingFields = [];

        if (!$('#companyName').val() || $('#companyName').val().trim() === '')
            missingFields.push('اسم الشركة');
        if (!$('#mainAddress').val() || $('#mainAddress').val().trim() === '')
            missingFields.push('العنوان الرئيسي');
        if (!$('#commercialRecord').val() || $('#commercialRecord').val().trim() === '')
            missingFields.push('رقم السجل التجاري');
        if (!$('#registrationExpiryDate').val())
            missingFields.push('تاريخ انتهاء قيد التسجيل');
        if (!$('#commercialRecordDate').val())
            missingFields.push('تاريخ التأشير بالسجل التجاري');
        if (!$('#registrationExpiryNotificationDays').val())
            missingFields.push('إشعار قبل انتهاء القيد');
        if (!$('#taxCard').val() || $('#taxCard').val().trim() === '')
            missingFields.push('رقم البطاقة الضريبية');
        if (!$('#taxCardRenewalDate').val())
            missingFields.push('تاريخ تجديد البطاقة الضريبية');
        if (!$('#taxCardNotificationDays').val())
            missingFields.push('إشعار قبل تجديد البطاقة');
        if (!$('#vatNumber').val() || $('#vatNumber').val().trim() === '')
            missingFields.push('ضريبة القيمة المضافة');
        if (!$('#auditor').val() || $('#auditor').val().trim() === '')
            missingFields.push('اسم مراقب الحسابات');
        if (!$('#auditorAppointmentDate').val())
            missingFields.push('تاريخ تعيين مراقب الحسابات');
        if (!$('#authorizedCapital').val())
            missingFields.push('رأس المال المرخص به');
        if (!$('#authorizedCapitalRecordDate').val())
            missingFields.push('تاريخ التأشير لرأس المال المرخص');
        if (!$('#issuedCapital').val())
            missingFields.push('رأس المال المصدر');
        if (!$('#issuedCapitalRecordDate').val())
            missingFields.push('تاريخ التأشير لرأس المال المصدر');
        if (!$('#paidCapital').val())
            missingFields.push('رأس المال المدفوع');
        if (!$('#paidCapitalRecordDate').val())
            missingFields.push('تاريخ التأشير لرأس المال المدفوع');

        const isValid = missingFields.length === 0;
        isCompanyInfoValid = isValid;

        if (isValid) {
            $('#company-info-tab').addClass('completed');
            $('#company-info-tab').removeClass('incomplete');
        } else {
            $('#company-info-tab').removeClass('completed');
            $('#company-info-tab').addClass('incomplete');
            window.currentMissingFields = missingFields;
        }
        updateSaveButtonState();
        return isValid;
    }

    function validateShareholders() {
        const hasActiveShareholder = shareholders.some(s => !s.endDate);
        const shareValue = parseFloat($('#fixedShareValue').val()) > 0;
        const isValid = hasActiveShareholder && shareValue;

        isShareholdersValid = isValid;

        if (isValid) {
            $('#shareholders-tab').addClass('completed');
            $('#shareholders-tab').removeClass('incomplete');
            $('#shareholdersValidationMsg').hide();
        } else {
            $('#shareholders-tab').removeClass('completed');
            $('#shareholders-tab').addClass('incomplete');
            $('#shareholdersValidationMsg').show();

            let missingFields = [];
            if (!hasActiveShareholder) missingFields.push('مساهم نشط واحد على الأقل');
            if (!shareValue) missingFields.push('قيمة السهم الواحد');
            window.currentMissingFields = missingFields;
        }
        updateSaveButtonState();
        return isValid;
    }

    function validateBranches() {
        const isValid = branches.length > 0;
        isBranchesValid = isValid;

        if (isValid) {
            $('#branches-tab').addClass('completed');
            $('#branches-tab').removeClass('incomplete');
            $('#branchesValidationMsg').hide();
        } else {
            $('#branches-tab').removeClass('completed');
            $('#branches-tab').addClass('incomplete');
            $('#branchesValidationMsg').show();
            window.currentMissingFields = ['فرع واحد على الأقل'];
        }
        updateSaveButtonState();
        return isValid;
    }
    function validateMeetings() {
        let missingFields = [];

        const ordinaryQuorum = $('#ordinaryMeetingQuorum').val();
        const extraordinaryQuorum = $('#extraordinaryMeetingQuorum').val();
        const boardQuorum = $('#boardMeetingQuorum').val();

        const ordinaryVoting = $('#ordinaryVotingMechanism').val();
        const extraordinaryVoting = $('#extraordinaryVotingMechanism').val();
        const boardVoting = $('#boardVotingMechanism').val();

        const ordinaryLocation = $('#ordinaryMeetingLocation').val();
        const extraordinaryLocation = $('#extraordinaryMeetingLocation').val();
        const boardLocation = $('#boardMeetingLocation').val();

        if (!ordinaryQuorum || ordinaryQuorum === '' || parseFloat(ordinaryQuorum) <= 0 || isNaN(parseFloat(ordinaryQuorum)))
            missingFields.push('نصاب الجمعية العامة العادية');
        if (!extraordinaryQuorum || extraordinaryQuorum === '' || parseFloat(extraordinaryQuorum) <= 0 || isNaN(parseFloat(extraordinaryQuorum)))
            missingFields.push('نصاب الجمعية العامة الغير عادية');
        if (!boardQuorum || boardQuorum === '' || parseFloat(boardQuorum) <= 0 || isNaN(parseFloat(boardQuorum)))
            missingFields.push('نصاب اجتماع مجلس الإدارة');

        if (!ordinaryVoting || ordinaryVoting === '')
            missingFields.push('آلية التصويت للجمعية العامة العادية');
        if (!extraordinaryVoting || extraordinaryVoting === '')
            missingFields.push('آلية التصويت للجمعية العامة الغير عادية');
        if (!boardVoting || boardVoting === '')
            missingFields.push('آلية التصويت لاجتماع مجلس الإدارة');

        if (!ordinaryLocation || ordinaryLocation.length === 0)
            missingFields.push('مكان انعقاد الجمعية العامة العادية');
        if (!extraordinaryLocation || extraordinaryLocation.length === 0)
            missingFields.push('مكان انعقاد الجمعية العامة الغير عادية');
        if (!boardLocation || boardLocation.length === 0)
            missingFields.push('مكان انعقاد اجتماع مجلس الإدارة');

        const isValid = missingFields.length === 0;
        isMeetingsValid = isValid;

        if (isValid) {
            $('#meetings-tab').addClass('completed');
            $('#meetings-tab').removeClass('incomplete');
        } else {
            $('#meetings-tab').removeClass('completed');
            $('#meetings-tab').addClass('incomplete');
            window.currentMissingFields = missingFields;
        }
        updateSaveButtonState();
        return isValid;
    }
   
    function validateBoardMembers() {
        let missingFields = [];

        const hasMembers = boardMembers.length > 0;
        const durationVal = $('#boardDuration').val();
        const startDateVal = $('#boardStartDate').val();

        if (!hasMembers) missingFields.push('عضو مجلس إدارة واحد على الأقل');
        if (!durationVal || durationVal.toString().trim() === '') missingFields.push('مدة المجلس');
        if (!startDateVal || startDateVal.toString().trim() === '') missingFields.push('تاريخ بداية المجلس');

        const isValid = hasMembers && durationVal && startDateVal;
        isBoardMembersValid = isValid;

        if (isValid) {
            $('#board-members-tab').addClass('completed');
            $('#board-members-tab').removeClass('incomplete');
            $('#boardMembersValidationMsg').hide();
        } else {
            $('#board-members-tab').removeClass('completed');
            $('#board-members-tab').addClass('incomplete');
            $('#boardMembersValidationMsg').show();
            window.currentMissingFields = missingFields;
        }
        updateSaveButtonState();
        return isValid;
    }

    function validateBankSignatures() {
        const isValid = bankSignatures.length > 0;
        isBankSignaturesValid = isValid;

        if (isValid) {
            $('#bank-signatures-tab').addClass('completed');
            $('#bank-signatures-tab').removeClass('incomplete');
            $('#bankSignaturesValidationMsg').hide();
        } else {
            $('#bank-signatures-tab').removeClass('completed');
            $('#bank-signatures-tab').addClass('incomplete');
            $('#bankSignaturesValidationMsg').show();
            window.currentMissingFields = ['مجموعة توقيع بنكية واحدة على الأقل'];
        }
        updateSaveButtonState();
        return isValid;
    }

    function validateNonBankSignatures() {
        const isValid = nonBankSignatures.length > 0;
        isNonBankSignaturesValid = isValid;

        if (isValid) {
            $('#non-bank-signatures-tab').addClass('completed');
            $('#non-bank-signatures-tab').removeClass('incomplete');
            $('#nonBankSignaturesValidationMsg').hide();
        } else {
            $('#non-bank-signatures-tab').removeClass('completed');
            $('#non-bank-signatures-tab').addClass('incomplete');
            $('#nonBankSignaturesValidationMsg').show();
            window.currentMissingFields = ['تفويض توقيع غير بنكي واحد على الأقل'];
        }
        updateSaveButtonState();
        return isValid;
    }
    // تحديث حالة زر حفظ الشركة
    function updateSaveButtonState() {
        const allValid = isCompanyInfoValid && isShareholdersValid && isBranchesValid &&
            isMeetingsValid && isBoardMembersValid && isBankSignaturesValid &&
            isNonBankSignaturesValid;

        const $saveBtn = $('#saveCompanyModalBtn');
        if (allValid) {
            $saveBtn.prop('disabled', false);
            $saveBtn.css('opacity', '1');
            $saveBtn.css('cursor', 'pointer');
        } else {
            $saveBtn.prop('disabled', true);
            $saveBtn.css('opacity', '0.5');
            $saveBtn.css('cursor', 'not-allowed');
        }
    }
    function forceRevalidateAllTabs() {
        validateCompanyInfo();
        validateShareholders();
        validateBranches();
        validateMeetings();
        validateBoardMembers();
        validateBankSignatures();
        validateNonBankSignatures();
        updateTabButtons();
       
    }

    function validateAllTabs() {
        return validateCompanyInfo() && validateShareholders() && validateBranches() &&
            validateMeetings() && validateBoardMembers() && validateBankSignatures() &&
            validateNonBankSignatures();
    }

    // ========== TABS NAVIGATION ==========
    let currentTab = 0;
    let isCompanyInfoValid = false;
    let isShareholdersValid = false;
    let isBranchesValid = false;
    let isMeetingsValid = false;
    let isBoardMembersValid = false;
    let isBankSignaturesValid = false;
    let isNonBankSignaturesValid = false;

    const tabs = ['#company-info-tab', '#shareholders-tab', '#branches-tab', '#meetings-tab', '#board-members-tab', '#bank-signatures-tab', '#non-bank-signatures-tab'];

    function updateTabButtons() {
        if (currentTab === 0) $('#prevTabBtn').hide();
        else $('#prevTabBtn').show();
        if (currentTab === tabs.length - 1) $('#nextTabBtn').html('<i class="fas fa-check"></i> حفظ');
        else $('#nextTabBtn').html('التالي <i class="fas fa-chevron-left"></i>');
    }

    function goToTab(index) {
        if (index >= 0 && index < tabs.length) {
            if (index > currentTab) {
                let canProceed = false;
                let tabName = "";
                let missingFields = [];

                if (currentTab === 0) {
                    canProceed = validateCompanyInfo();
                    tabName = "بيانات الشركة";
                    if (!canProceed && window.currentMissingFields) {
                        missingFields = window.currentMissingFields;
                    }
                }
                else if (currentTab === 1) {
                    canProceed = validateShareholders();
                    tabName = "المساهمين";
                    if (!canProceed && window.currentMissingFields) {
                        missingFields = window.currentMissingFields;
                    }
                }
                else if (currentTab === 2) {
                    canProceed = validateBranches();
                    tabName = "الفروع";
                    if (!canProceed && window.currentMissingFields) {
                        missingFields = window.currentMissingFields;
                    }
                }
                else if (currentTab === 3) {
                    canProceed = validateMeetings();
                    tabName = "الاجتماعات";
                    if (!canProceed && window.currentMissingFields) {
                        missingFields = window.currentMissingFields;
                    }
                }
                else if (currentTab === 4) {
                    canProceed = validateBoardMembers();
                    tabName = "أعضاء المجلس";
                    if (!canProceed && window.currentMissingFields) {
                        missingFields = window.currentMissingFields;
                    }
                }
                else if (currentTab === 5) {
                    canProceed = validateBankSignatures();
                    tabName = "صلاحيات التوقيع البنكية";
                    if (!canProceed && window.currentMissingFields) {
                        missingFields = window.currentMissingFields;
                    }
                }
                else if (currentTab === 6) {
                    canProceed = validateNonBankSignatures();
                    tabName = "صلاحيات التوقيع غير البنكية";
                    if (!canProceed && window.currentMissingFields) {
                        missingFields = window.currentMissingFields;
                    }
                }

                if (!canProceed) {
                    if (missingFields.length > 0) {
                        let fieldsList = missingFields.join('، ');
                        showToast(`يرجى إكمال البيانات المطلوبة في تاب "${tabName}": ${fieldsList}`, 'danger');
                    } else {
                        showToast(`يرجى إكمال جميع البيانات المطلوبة في تاب "${tabName}" أولاً`, 'danger');
                    }
                    return;
                }
            }
            $(tabs[index]).tab('show');
            currentTab = index;
            updateTabButtons();
            updateSaveButtonState();
        }
    }
    $('#nextTabBtn').click(function () {
        if (currentTab !== tabs.length - 1) goToTab(currentTab + 1);
        else saveCompany();
    });
    $('#prevTabBtn').click(function () { goToTab(currentTab - 1); });

    function resetAllForms() {
        $('#companyInfoForm')[0].reset();
        $('#editingCompanyId').val('');
        shareholders = [];
        branches = [];
        boardMembers = [];
        bankSignatures = [];
        nonBankSignatures = [];
        $('#fixedShareValue').val('1.00');
        resetIdCounters();
        $('#ordinaryMeetingLocation').val([]);
        $('#ordinaryMeetingAddress').val('');
        $('#ordinaryMeetingQuorum').val('');
        $('#ordinaryVotingMechanism').val('');
        $('#extraordinaryMeetingLocation').val([]);
        $('#extraordinaryMeetingAddress').val('');
        $('#extraordinaryMeetingQuorum').val('');
        $('#extraordinaryVotingMechanism').val('');
        $('#boardMeetingLocation').val([]);
        $('#boardMeetingAddress').val('');
        $('#boardMeetingQuorum').val('');
        $('#boardVotingMechanism').val('');
        $('#boardDuration').val('');
        $('#boardStartDate').val('');
        $('#boardExpiryNotificationDays').val('');
        $('#bankSignatureGroupSelect').val('');
        $('#authorizedPersons').val('');
        $('#authorizedCapitalRecordDate').val('');
        $('#issuedCapitalRecordDate').val('');
        $('#paidCapitalRecordDate').val('');
        $('#taxCardRenewalDate').val('');
        $('#auditorAppointmentDate').val('');
        $('#registrationExpiryNotificationDays').val('');
        $('#taxCardNotificationDays').val('');
        $('#bankSignaturesNotes').val('');
        $('#nonBankSignaturesNotes').val('');
        renderShareholders();
        renderBranches();
        renderBoardMembers();
        renderBankSignatures();
        renderNonBankSignatures();
        isCompanyInfoValid = false;
        isShareholdersValid = false;
        isBranchesValid = false;
        isMeetingsValid = false;
        isBoardMembersValid = false;
        isBankSignaturesValid = false;
        isNonBankSignaturesValid = false;
        $('.nav-link').removeClass('completed');
        $('#company-info-tab').addClass('active');
    }

    function loadBankGroups() {
        ajaxRequest(API.getBankGroups, 'GET', null, function (groups) {
            const $select = $('#bankSignatureGroupSelect');
            $select.empty().append('<option value="">-- اختر مجموعة --</option>');
            groups.forEach(function (group) {
                $select.append(`<option value="${escapeHtml(group)}">${escapeHtml(group)}</option>`);
            });
        });
    }
    // جلب قيمة السهم من الخادم
    function loadShareValue() {
        ajaxRequest(API.getShareValue, 'GET', null, function (response) {
            if (response.shareValue !== undefined && response.shareValue !== null) {
                $('#fixedShareValue').val(response.shareValue);
                // إعادة حساب قيم المساهمين بعد تحديث قيمة السهم
                calculateShareholderValues();
                validateShareholders();
            } else {
                $('#fixedShareValue').val(1.00);
            }
        }, function (error) {
            console.error('Failed to load share value:', error);
            $('#fixedShareValue').val(1.00);
        });
    }

    // ========== EVENT LISTENERS ==========
    
    $('#addNewCompanyBtn').click(function () {
        editingCompanyId = null;
        $('#companyModalTitle').text('إضافة شركة جديدة');
        resetAllForms();
        loadShareValue();
        currentTab = 0;
        updateTabButtons();

        // إعادة التحقق من جميع التابات بعد reset
        setTimeout(function () {
            forceRevalidateAllTabs();
        }, 100);

        goToTab(0);
        loadBankGroups();
        $('#companyModal').modal('show');
    });
    $('#saveCompanyModalBtn').click(function () {
        //console.log('Save button clicked');
        saveCompany();
    });
    $('#companyInfoForm input, #companyInfoForm select').on('input change', function () { validateCompanyInfo(); });
    $('#ordinaryMeetingQuorum, #extraordinaryMeetingQuorum, #boardMeetingQuorum').on('input change', function () { validateMeetings(); });
    $('#boardDuration, #boardStartDate').on('input change', function () { validateBoardMembers(); });
    $('#fixedShareValue').on('input change', function () { calculateShareholderValues(); validateShareholders(); });
    $('#subscribedShares, #preferredShares, #founderShares').on('input', calculateShareholderValues);

    // ========== INITIALIZE ==========
    loadCompanies();
    loadBankGroups();
    loadShareValue();
    if ($('.toast-container').length === 0) {
        $('body').append('<div class="toast-container position-fixed top-0 end-0 p-3" style="z-index: 9999;"></div>');
    }
    // ========== SEARCH AND FILTER FUNCTIONALITY ==========

    let filteredCompanies = [];
    let currentSearchTerm = '';
    let currentFilterType = 'all';
    let currentStatusFilter = 'all';
    let isAdvancedSearchActive = false;
    let advancedSearchCriteria = {};

    // Search function
    function filterCompanies() {
        let results = [...companies];

        // تطبيق البحث المتقدم أولاً إذا كان نشطاً
        if (isAdvancedSearchActive && Object.keys(advancedSearchCriteria).length > 0) {
            results = applyAdvancedFilter(results);
        }

        // تطبيق البحث البسيط
        if (currentSearchTerm) {
            const searchLower = currentSearchTerm.toLowerCase();

            results = results.filter(company => {
                let matchFound = false;

                switch (currentFilterType) {
                    case 'name':
                        matchFound = (company.name || '').toLowerCase().includes(searchLower);
                        break;
                    case 'commercial':
                        matchFound = (company.commercialRegNo || '').toLowerCase().includes(searchLower);
                        break;
                    case 'address':
                        matchFound = (company.headOfficeAddress || '').toLowerCase().includes(searchLower);
                        break;
                    case 'capital':
                        matchFound = (company.authorizedCapital || 0).toString().includes(searchLower);
                        break;
                    case 'shareholders':
                        matchFound = (company.shareholders?.length || 0).toString().includes(searchLower);
                        break;
                    case 'branches':
                        matchFound = (company.branches?.length || 0).toString().includes(searchLower);
                        break;
                    default:
                        // بحث في جميع الحقول
                        matchFound = (company.name || '').toLowerCase().includes(searchLower) ||
                            (company.commercialRegNo || '').toLowerCase().includes(searchLower) ||
                            (company.headOfficeAddress || '').toLowerCase().includes(searchLower) ||
                            (company.authorizedCapital || 0).toString().includes(searchLower) ||
                            (company.shareholders?.length || 0).toString().includes(searchLower) ||
                            (company.branches?.length || 0).toString().includes(searchLower);
                        break;
                }
                return matchFound;
            });
        }

        // تطبيق تصفية الحالة
        if (currentStatusFilter !== 'all') {
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            const thirtyDaysFromNow = new Date(today);
            thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);

            results = results.filter(company => {
                const expiryDate = company.registrationExpiry ? new Date(company.registrationExpiry) : null;

                if (currentStatusFilter === 'active') {
                    return expiryDate && expiryDate > today;
                } else if (currentStatusFilter === 'expiring') {
                    return expiryDate && expiryDate > today && expiryDate <= thirtyDaysFromNow;
                } else if (currentStatusFilter === 'expired') {
                    return expiryDate && expiryDate <= today;
                }
                return true;
            });
        }

        filteredCompanies = results;
        updateSearchStats(results.length);
        renderFilteredTable(results);
    }

    // تطبيق البحث المتقدم
    function applyAdvancedFilter(results) {
        return results.filter(company => {
            let match = true;

            // اسم الشركة
            if (advancedSearchCriteria.name && advancedSearchCriteria.name.trim()) {
                const nameLower = advancedSearchCriteria.name.toLowerCase();
                if (!(company.name || '').toLowerCase().includes(nameLower)) match = false;
            }

            // رقم السجل التجاري
            if (match && advancedSearchCriteria.commercial && advancedSearchCriteria.commercial.trim()) {
                const commercialLower = advancedSearchCriteria.commercial.toLowerCase();
                if (!(company.commercialRegNo || '').toLowerCase().includes(commercialLower)) match = false;
            }

            // العنوان
            if (match && advancedSearchCriteria.address && advancedSearchCriteria.address.trim()) {
                const addressLower = advancedSearchCriteria.address.toLowerCase();
                if (!(company.headOfficeAddress || '').toLowerCase().includes(addressLower)) match = false;
            }

            // رأس المال (من - إلى)
            if (match && (advancedSearchCriteria.capitalMin || advancedSearchCriteria.capitalMax)) {
                const capital = company.authorizedCapital || 0;
                if (advancedSearchCriteria.capitalMin && capital < parseFloat(advancedSearchCriteria.capitalMin)) match = false;
                if (match && advancedSearchCriteria.capitalMax && capital > parseFloat(advancedSearchCriteria.capitalMax)) match = false;
            }

            // عدد المساهمين (من - إلى)
            if (match && (advancedSearchCriteria.shareholdersMin || advancedSearchCriteria.shareholdersMax)) {
                const shareholderCount = company.shareholders?.length || 0;
                if (advancedSearchCriteria.shareholdersMin && shareholderCount < parseInt(advancedSearchCriteria.shareholdersMin)) match = false;
                if (match && advancedSearchCriteria.shareholdersMax && shareholderCount > parseInt(advancedSearchCriteria.shareholdersMax)) match = false;
            }

            // عدد الفروع (من - إلى)
            if (match && (advancedSearchCriteria.branchesMin || advancedSearchCriteria.branchesMax)) {
                const branchCount = company.branches?.length || 0;
                if (advancedSearchCriteria.branchesMin && branchCount < parseInt(advancedSearchCriteria.branchesMin)) match = false;
                if (match && advancedSearchCriteria.branchesMax && branchCount > parseInt(advancedSearchCriteria.branchesMax)) match = false;
            }

            // تاريخ التسجيل (من - إلى)
            if (match && (advancedSearchCriteria.dateFrom || advancedSearchCriteria.dateTo)) {
                const regDate = company.registrationDate ? new Date(company.registrationDate) : null;
                if (regDate) {
                    if (advancedSearchCriteria.dateFrom && regDate < new Date(advancedSearchCriteria.dateFrom)) match = false;
                    if (match && advancedSearchCriteria.dateTo && regDate > new Date(advancedSearchCriteria.dateTo)) match = false;
                } else {
                    match = false;
                }
            }

            // حالة التسجيل
            if (match && advancedSearchCriteria.status && advancedSearchCriteria.status !== 'all') {
                const today = new Date();
                today.setHours(0, 0, 0, 0);
                const thirtyDaysFromNow = new Date(today);
                thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);
                const expiryDate = company.registrationExpiry ? new Date(company.registrationExpiry) : null;

                if (advancedSearchCriteria.status === 'active') {
                    match = expiryDate && expiryDate > today;
                } else if (advancedSearchCriteria.status === 'expiring') {
                    match = expiryDate && expiryDate > today && expiryDate <= thirtyDaysFromNow;
                } else if (advancedSearchCriteria.status === 'expired') {
                    match = expiryDate && expiryDate <= today;
                }
            }

            return match;
        });
    }

    // تحديث إحصائيات البحث
    function updateSearchStats(count) {
        const total = companies.length;
        const searchStats = $('#searchStats');

        if (currentSearchTerm || currentStatusFilter !== 'all' || isAdvancedSearchActive) {
            searchStats.html(`
            <i class="fas fa-filter me-1"></i>
            تم العثور على <strong>${count}</strong> شركة من أصل <strong>${total}</strong>
            ${currentSearchTerm ? ` - بحث: "${currentSearchTerm}"` : ''}
            ${currentStatusFilter !== 'all' ? ` - الحالة: ${getStatusText(currentStatusFilter)}` : ''}
            ${isAdvancedSearchActive ? ' - <span style="color: #65b8b5;">بحث متقدم نشط</span>' : ''}
        `).show();
        } else {
            searchStats.html(`<i class="fas fa-building me-1"></i> إجمالي الشركات: <strong>${total}</strong>`).show();
        }
    }

    function getStatusText(status) {
        const statusMap = {
            'active': 'نشطة',
            'expiring': 'على وشك الانتهاء',
            'expired': 'منتهية'
        };
        return statusMap[status] || status;
    }

    // عرض الجدول المصفى مع إمكانية تمييز النص
    //function renderFilteredTable(companiesToRender) {
    //    if (!companiesToRender || companiesToRender.length === 0) {
    //        $('#companiesTableBody').html(`
    //        <tr>
    //            <td colspan="8" class="text-center text-muted py-4">
    //                <i class="fas fa-search fa-2x mb-2 d-block" style="color: #7bc6c3;"></i>
    //                لا توجد نتائج تطابق معايير البحث
    //                <div class="mt-2">
    //                    <button class="btn btn-sm btn-teal" id="clearSearchAndReset">
    //                        <i class="fas fa-undo-alt me-1"></i> إعادة تعيين البحث
    //                    </button>
    //                </div>
    //            </td>
    //        </td>
    //    `);

    //        $('#clearSearchAndReset').off('click').on('click', function () {
    //            resetAllFilters();
    //        });
    //        return;
    //    }

    //    let html = '';
    //    companiesToRender.forEach(function (company, index) {
    //        // Highlight search term if exists
    //        let companyName = company.name || '';
    //        let commercialRegNo = company.commercialRegNo || '';
    //        let headOfficeAddress = company.headOfficeAddress || '';

    //        if (currentSearchTerm && currentFilterType === 'all') {
    //            const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
    //            companyName = companyName.replace(regex, '<span class="search-highlight">$1</span>');
    //            commercialRegNo = commercialRegNo.replace(regex, '<span class="search-highlight">$1</span>');
    //            headOfficeAddress = headOfficeAddress.replace(regex, '<span class="search-highlight">$1</span>');
    //        } else if (currentSearchTerm && currentFilterType === 'name') {
    //            const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
    //            companyName = companyName.replace(regex, '<span class="search-highlight">$1</span>');
    //        } else if (currentSearchTerm && currentFilterType === 'commercial') {
    //            const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
    //            commercialRegNo = commercialRegNo.replace(regex, '<span class="search-highlight">$1</span>');
    //        } else if (currentSearchTerm && currentFilterType === 'address') {
    //            const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
    //            headOfficeAddress = headOfficeAddress.replace(regex, '<span class="search-highlight">$1</span>');
    //        }

    //        // Get status badge
    //        let statusBadge = getStatusBadge(company);

    //        html += `
    //        <tr>
    //            <td class="text-center">${index + 1}${statusBadge ? '<br>' + statusBadge : ''}</td>
    //            <td class="text-center">${companyName || '-'}</td>
    //            <td class="text-center">${commercialRegNo || '-'}</td>
    //            <td class="text-center">${headOfficeAddress || '-'}</td>
    //            <td class="text-center">${(company.authorizedCapital || 0).toLocaleString()} ${(company.authorizedCapital ? 'ج.م' : '')}</td>
    //            <td class="text-center">${company.shareholders?.length || 0}</td>
    //            <td class="text-center">${company.branches?.length || 0}</td>
    //            <td class="text-center">
    //                <div class="action-buttons">
    //                    <button class="btn-action btn-view" onclick="viewCompanyDetails(${company.companyId})" title="عرض التفاصيل">
    //                        <i class="fas fa-eye"></i>
    //                    </button>
    //                    <button class="btn-action btn-edit" onclick="editCompany(${company.companyId})" title="تعديل">
    //                        <i class="fas fa-edit"></i>
    //                    </button>
    //                    <button class="btn-action btn-attach" onclick="manageAttachments(${company.companyId})" title="إدارة المرفقات">
    //                        <i class="fas fa-paperclip"></i>
    //                    </button>
    //                    <button class="btn-action btn-delete" onclick="deleteCompany(${company.companyId})" title="حذف">
    //                        <i class="fas fa-trash-alt"></i>
    //                    </button>
    //                </div>
    //            </td>
    //         </tr>
    //    `;
    //    });
    //    $('#companiesTableBody').html(html);
    //}


    // عرض الجدول المصفى مع إمكانية تمييز النص
    function renderFilteredTable(companiesToRender) {
        if (!companiesToRender || companiesToRender.length === 0) {
            $('#companiesTableBody').html(`
            <tr>
                <td colspan="8" class="text-center text-muted py-4">
                    <i class="fas fa-search fa-2x mb-2 d-block" style="color: #7bc6c3;"></i>
                    لا توجد نتائج تطابق معايير البحث
                    <div class="mt-2">
                        <button class="btn btn-sm btn-teal" id="clearSearchAndReset">
                            <i class="fas fa-undo-alt me-1"></i> إعادة تعيين البحث
                        </button>
                    </div>
                </td>
            </tr>
        `);

            $('#clearSearchAndReset').off('click').on('click', function () {
                resetAllFilters();
            });
            return;
        }

        let html = '';
        companiesToRender.forEach(function (company, index) {
            // Highlight search term if exists
            let companyName = company.name || '';
            let commercialRegNo = company.commercialRegNo || '';

            if (currentSearchTerm && currentFilterType === 'all') {
                const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
                companyName = companyName.replace(regex, '<span class="search-highlight">$1</span>');
                commercialRegNo = commercialRegNo.replace(regex, '<span class="search-highlight">$1</span>');
            } else if (currentSearchTerm && currentFilterType === 'name') {
                const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
                companyName = companyName.replace(regex, '<span class="search-highlight">$1</span>');
            } else if (currentSearchTerm && currentFilterType === 'commercial') {
                const regex = new RegExp(`(${escapeRegex(currentSearchTerm)})`, 'gi');
                commercialRegNo = commercialRegNo.replace(regex, '<span class="search-highlight">$1</span>');
            }

            // Get status badge
            let statusBadge = getStatusBadge(company);

            html += `
            <tr>
                <td class="text-center">${index + 1}</td>
                <td class="text-center">${companyName || '-'}</td>
                <td class="text-center">${commercialRegNo || '-'}</td>
                <td class="text-center">${statusBadge}</td>
                <td class="text-center">${(company.authorizedCapital || 0).toLocaleString()} ${(company.authorizedCapital ? 'ج.م' : '')}</td>
                <td class="text-center">${company.shareholders?.length || 0}</td>
                <td class="text-center">${company.branches?.length || 0}</td>
                <td class="text-center">
                    <div class="action-buttons">
                        <button class="btn-action btn-view" onclick="viewCompanyDetails(${company.companyId})" title="عرض التفاصيل">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn-action btn-edit" onclick="editCompany(${company.companyId})" title="تعديل">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="btn-action btn-attach" onclick="manageAttachments(${company.companyId})" title="إدارة المرفقات">
                            <i class="fas fa-paperclip"></i>
                        </button>
                        <button class="btn-action btn-delete" onclick="deleteCompany(${company.companyId})" title="حذف">
                            <i class="fas fa-trash-alt"></i>
                        </button>
                    </div>
                </td>
             </tr>
        `;
        });
        $('#companiesTableBody').html(html);
    }
    //function getStatusBadge(company) {
    //    if (!company.registrationExpiry) return null;

    //    const today = new Date();
    //    today.setHours(0, 0, 0, 0);
    //    const expiryDate = new Date(company.registrationExpiry);
    //    expiryDate.setHours(0, 0, 0, 0);
    //    const thirtyDaysFromNow = new Date(today);
    //    thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);

    //    if (expiryDate <= today) {
    //        return '<span class="badge bg-danger" style="font-size: 10px;"><i class="fas fa-ban"></i> منتهية (منذ ${daysOver} يوم)</span>';
    //    } else if (expiryDate <= thirtyDaysFromNow) {
    //        return '<span class="badge bg-warning" style="font-size: 10px;">على وشك الانتهاء</span>';
    //    } else {
    //        return '<span class="badge bg-success" style="font-size: 10px;">نشطة</span>';
    //    }
    //}

    function getStatusBadge(company) {
        if (!company.registrationExpiry) {
            return '<span class="status-badge status-no-expiry"><i class="fas fa-question-circle"></i> غير محدد</span>';
        }

        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const expiryDate = new Date(company.registrationExpiry);
        expiryDate.setHours(0, 0, 0, 0);
        const thirtyDaysFromNow = new Date(today);
        thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);

        if (expiryDate <= today) {
            const daysOver = Math.floor((today - expiryDate) / (1000 * 60 * 60 * 24));
            return `<span class="badge bg-danger" style="font-size: 10px;><i class="fas fa-ban"></i> منتهية (منذ ${daysOver} يوم)</span>`;
        } else if (expiryDate <= thirtyDaysFromNow) {
            const daysLeft = Math.floor((expiryDate - today) / (1000 * 60 * 60 * 24));
            return `<span class="badge bg-warning" style="font-size: 10px;"><i class="fas fa-hourglass-half"></i> على وشك الانتهاء (${daysLeft} يوم متبقي)</span>`;
        } else {
            const daysLeft = Math.floor((expiryDate - today) / (1000 * 60 * 60 * 24));
            return `<span class="badge bg-success" style="font-size: 10px;"><i class="fas fa-check-circle"></i> نشطة (${daysLeft} يوم متبقي)</span>`;
        }
    }
    function escapeRegex(string) {
        return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    // Reset all filters
    function resetAllFilters() {
        currentSearchTerm = '';
        currentFilterType = 'all';
        currentStatusFilter = 'all';
        isAdvancedSearchActive = false;
        advancedSearchCriteria = {};

        $('#searchInput').val('');
        $('#filterType').val('all');
        $('#statusFilter').val('all');

        // Clear advanced search fields
        $('#advName, #advCommercial, #advAddress, #advCapitalMin, #advCapitalMax, #advShareholdersMin, #advShareholdersMax, #advBranchesMin, #advBranchesMax, #advDateFrom, #advDateTo').val('');
        $('#advStatus').val('all');
        $('#advancedSearchPanel').hide();
        $('#advancedSearchBtn').html('<i class="fas fa-sliders-h me-1"></i> بحث متقدم');

        filteredCompanies = [...companies];
        updateSearchStats(companies.length);
        renderFilteredTable(companies);
        showToast('تم إعادة تعيين جميع عوامل التصفية', 'info');
    }

    // Event Listeners for Search
    $(document).ready(function () {
        // Simple search input
        $('#searchInput').on('input', function () {
            currentSearchTerm = $(this).val();
            filterCompanies();
        });

        // Filter type change
        $('#filterType').on('change', function () {
            currentFilterType = $(this).val();
            filterCompanies();
        });

        // Status filter change
        $('#statusFilter').on('change', function () {
            currentStatusFilter = $(this).val();
            filterCompanies();
        });

        // Advanced search toggle
        $('#advancedSearchBtn').click(function () {
            $('#advancedSearchPanel').slideToggle();
            const btn = $(this);
            if ($('#advancedSearchPanel').is(':visible')) {
                btn.html('<i class="fas fa-chevron-up me-1"></i> إخفاء البحث المتقدم');
            } else {
                btn.html('<i class="fas fa-sliders-h me-1"></i> بحث متقدم');
            }
        });

        // Apply advanced search
        $('#applyAdvancedSearch').click(function () {
            advancedSearchCriteria = {
                name: $('#advName').val(),
                commercial: $('#advCommercial').val(),
                address: $('#advAddress').val(),
                capitalMin: $('#advCapitalMin').val(),
                capitalMax: $('#advCapitalMax').val(),
                shareholdersMin: $('#advShareholdersMin').val(),
                shareholdersMax: $('#advShareholdersMax').val(),
                branchesMin: $('#advBranchesMin').val(),
                branchesMax: $('#advBranchesMax').val(),
                dateFrom: $('#advDateFrom').val(),
                dateTo: $('#advDateTo').val(),
                status: $('#advStatus').val()
            };

            isAdvancedSearchActive = true;
            filterCompanies();
            showToast('تم تطبيق البحث المتقدم', 'success');
        });

        // Clear advanced search
        $('#clearAdvancedSearch').click(function () {
            $('#advName, #advCommercial, #advAddress, #advCapitalMin, #advCapitalMax, #advShareholdersMin, #advShareholdersMax, #advBranchesMin, #advBranchesMax, #advDateFrom, #advDateTo').val('');
            $('#advStatus').val('all');
            advancedSearchCriteria = {};
            isAdvancedSearchActive = false;
            filterCompanies();
            showToast('تم مسح معايير البحث المتقدم', 'info');
        });

        // Reset search button
        $('#resetSearchBtn').click(function () {
            resetAllFilters();
        });
    });
});