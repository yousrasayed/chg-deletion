using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CHG_Legal.Migrations
{
    /// <inheritdoc />
    public partial class FinalAttachmentServiceFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Company");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Association",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssociationType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    VotingMechanism = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ValidityValue = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Association", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Attendees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attendee = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Job_Describtion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendees", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BoardType",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardType", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                schema: "Company",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HeadOfficeAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CommercialRegNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegistrationExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TaxCardNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AuthorizedCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IssuedCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AuditorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CardRenewalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CardRenewalDue = table.Column<int>(type: "int", nullable: true),
                    AccountingAuditor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AccountingAuditorHiringDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AuthorizedCapitalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedCapitalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidUpCapital = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaidUpCapitalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegistrationNotificationPeriod = table.Column<int>(type: "int", nullable: true),
                    VAT = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                schema: "Company",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Hospitals",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Hospital = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hospitals", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "ShareValue",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShareValue = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareValue", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    User_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Name = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.User_ID);
                });

            migrationBuilder.CreateTable(
                name: "AssociationPlace",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AsscoiationPlace = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    associationID = table.Column<int>(type: "int", nullable: true),
                    associationPlaceDecr = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociationPlace", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AssociationPlace_Association_associationID",
                        column: x => x.associationID,
                        principalTable: "Association",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankingGroups",
                schema: "Company",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingGroups", x => x.GroupId);
                    table.ForeignKey(
                        name: "FK_BankingGroups_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardSettings",
                schema: "Company",
                columns: table => new
                {
                    BoardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificationPeriod = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardSettings", x => x.BoardId);
                    table.ForeignKey(
                        name: "FK_BoardSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                schema: "Company",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RegistrationNotification = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.BranchId);
                    table.ForeignKey(
                        name: "FK_Branches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyAttachments",
                schema: "Company",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    file_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    file_data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAttachments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CompanyAttachments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NonBankingAuthorizations",
                schema: "Company",
                columns: table => new
                {
                    AuthorizationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssuedTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AuthorizationDetails = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    NotificationPerid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonBankingAuthorizations", x => x.AuthorizationId);
                    table.ForeignKey(
                        name: "FK_NonBankingAuthorizations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shareholders",
                schema: "Company",
                columns: table => new
                {
                    ShareholderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    ShareName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SharesPercentage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FounderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FounderShareCount = table.Column<double>(type: "float", nullable: true),
                    SubscribedShareCount = table.Column<double>(type: "float", nullable: true),
                    ExcellentShareCount = table.Column<double>(type: "float", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalShareCounts = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shareholders", x => x.ShareholderId);
                    table.ForeignKey(
                        name: "FK_Shareholders_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hospital_Attendees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attendee_ID = table.Column<int>(type: "int", nullable: false),
                    Hospital_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hospital_Attendees", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Hospital_Attendees_Attendees_Attendee_ID",
                        column: x => x.Attendee_ID,
                        principalTable: "Attendees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Hospital_Attendees_Hospitals_Hospital_ID",
                        column: x => x.Hospital_ID,
                        principalTable: "Hospitals",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Board",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Attendees = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    BoardDescsions = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CHGParty = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MeetingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MeetingStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BoardTypeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Board", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Board_BoardType_BoardTypeID",
                        column: x => x.BoardTypeID,
                        principalTable: "BoardType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Board_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "User_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserRole_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRole_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "User_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankingAttachments",
                schema: "Company",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    MemberName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    file_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    file_data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    BankingGroupGroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingAttachments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BankingAttachments_BankingGroups_BankingGroupGroupId",
                        column: x => x.BankingGroupGroupId,
                        principalSchema: "Company",
                        principalTable: "BankingGroups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_BankingAttachments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankingGroupMembers",
                schema: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    MemberName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankingGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankingGroupMembers_BankingGroups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "Company",
                        principalTable: "BankingGroups",
                        principalColumn: "GroupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardMembers",
                schema: "Company",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardMembers", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_BoardMembers_BoardSettings_BoardId",
                        column: x => x.BoardId,
                        principalSchema: "Company",
                        principalTable: "BoardSettings",
                        principalColumn: "BoardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BranchAttachments",
                schema: "Company",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    file_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    file_data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchAttachments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BranchAttachments_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Company",
                        principalTable: "Branches",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchAttachments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NonBankingAttaches",
                schema: "Company",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorizationId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    file_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    file_data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonBankingAttaches", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NonBankingAttaches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NonBankingAttaches_NonBankingAuthorizations_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalSchema: "Company",
                        principalTable: "NonBankingAuthorizations",
                        principalColumn: "AuthorizationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShareHolder_Attaches",
                schema: "dbo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShareholderId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    file_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    file_data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareHolder_Attaches", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ShareHolder_Attaches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "Company",
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShareHolder_Attaches_Shareholders_ShareholderId",
                        column: x => x.ShareholderId,
                        principalSchema: "Company",
                        principalTable: "Shareholders",
                        principalColumn: "ShareholderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Approves",
                columns: table => new
                {
                    ApprovalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovedBy = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BoardApprovalID = table.Column<int>(type: "int", nullable: false),
                    Attendee_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approves", x => x.ApprovalID);
                    table.ForeignKey(
                        name: "FK_Approves_Attendees_Attendee_ID",
                        column: x => x.Attendee_ID,
                        principalTable: "Attendees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Approves_Board_BoardApprovalID",
                        column: x => x.BoardApprovalID,
                        principalTable: "Board",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Board_Attachments",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Board_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    file_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    file_data = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Board_Attachments", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Board_Attachments_Board_Board_id",
                        column: x => x.Board_id,
                        principalTable: "Board",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Board_Attachments_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "User_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardAttendees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovedBy = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BoardID = table.Column<int>(type: "int", nullable: false),
                    Attendee_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardAttendees", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BoardAttendees_Attendees_Attendee_ID",
                        column: x => x.Attendee_ID,
                        principalTable: "Attendees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardAttendees_Board_BoardID",
                        column: x => x.BoardID,
                        principalTable: "Board",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardDecisions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Board_ID = table.Column<int>(type: "int", nullable: false),
                    Decesion_Number = table.Column<short>(type: "smallint", nullable: false),
                    Decesion_Details = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: false),
                    IsExcuted = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    DateInserted = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardDecisions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BoardDecisions_Board_Board_ID",
                        column: x => x.Board_ID,
                        principalTable: "Board",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BoardMemberAttaches",
                schema: "Company",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberId = table.Column<int>(type: "int", nullable: true),
                    file_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    file_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    file_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    file_data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    BoardSettingBoardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardMemberAttaches", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BoardMemberAttaches_BoardMembers_MemberId",
                        column: x => x.MemberId,
                        principalSchema: "Company",
                        principalTable: "BoardMembers",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoardMemberAttaches_BoardSettings_BoardSettingBoardId",
                        column: x => x.BoardSettingBoardId,
                        principalSchema: "Company",
                        principalTable: "BoardSettings",
                        principalColumn: "BoardId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Approves_Attendee_ID",
                table: "Approves",
                column: "Attendee_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Approves_BoardApprovalID",
                table: "Approves",
                column: "BoardApprovalID");

            migrationBuilder.CreateIndex(
                name: "IX_AssociationPlace_associationID",
                table: "AssociationPlace",
                column: "associationID");

            migrationBuilder.CreateIndex(
                name: "IX_BankingAttachments_BankingGroupGroupId",
                schema: "Company",
                table: "BankingAttachments",
                column: "BankingGroupGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_BankingAttachments_CompanyId",
                schema: "Company",
                table: "BankingAttachments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BankingGroupMembers_GroupId",
                schema: "Company",
                table: "BankingGroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_BankingGroups_CompanyId",
                schema: "Company",
                table: "BankingGroups",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Board_BoardTypeID",
                table: "Board",
                column: "BoardTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Board_UserID",
                table: "Board",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Board_Attachments_Board_id",
                table: "Board_Attachments",
                column: "Board_id");

            migrationBuilder.CreateIndex(
                name: "IX_Board_Attachments_user_id",
                table: "Board_Attachments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_BoardAttendees_Attendee_ID",
                table: "BoardAttendees",
                column: "Attendee_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BoardAttendees_BoardID",
                table: "BoardAttendees",
                column: "BoardID");

            migrationBuilder.CreateIndex(
                name: "IX_BoardDecisions_Board_ID",
                table: "BoardDecisions",
                column: "Board_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BoardMemberAttaches_BoardSettingBoardId",
                schema: "Company",
                table: "BoardMemberAttaches",
                column: "BoardSettingBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardMemberAttaches_MemberId",
                schema: "Company",
                table: "BoardMemberAttaches",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardMembers_BoardId",
                schema: "Company",
                table: "BoardMembers",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardSettings_CompanyId",
                schema: "Company",
                table: "BoardSettings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchAttachments_BranchId",
                schema: "Company",
                table: "BranchAttachments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchAttachments_CompanyId",
                schema: "Company",
                table: "BranchAttachments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_CompanyId",
                schema: "Company",
                table: "Branches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAttachments_CompanyId",
                schema: "Company",
                table: "CompanyAttachments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Hospital_Attendees_Attendee_ID",
                table: "Hospital_Attendees",
                column: "Attendee_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Hospital_Attendees_Hospital_ID",
                table: "Hospital_Attendees",
                column: "Hospital_ID");

            migrationBuilder.CreateIndex(
                name: "IX_NonBankingAttaches_AuthorizationId",
                schema: "Company",
                table: "NonBankingAttaches",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_NonBankingAttaches_CompanyId",
                schema: "Company",
                table: "NonBankingAttaches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_NonBankingAuthorizations_CompanyId",
                schema: "Company",
                table: "NonBankingAuthorizations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareHolder_Attaches_CompanyId",
                schema: "dbo",
                table: "ShareHolder_Attaches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareHolder_Attaches_ShareholderId",
                schema: "dbo",
                table: "ShareHolder_Attaches",
                column: "ShareholderId");

            migrationBuilder.CreateIndex(
                name: "IX_Shareholders_CompanyId",
                schema: "Company",
                table: "Shareholders",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleID",
                table: "UserRole",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserID",
                table: "UserRole",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Approves");

            migrationBuilder.DropTable(
                name: "AssociationPlace");

            migrationBuilder.DropTable(
                name: "BankingAttachments",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "BankingGroupMembers",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "Board_Attachments");

            migrationBuilder.DropTable(
                name: "BoardAttendees");

            migrationBuilder.DropTable(
                name: "BoardDecisions");

            migrationBuilder.DropTable(
                name: "BoardMemberAttaches",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "BranchAttachments",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "CompanyAttachments",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "Groups",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "Hospital_Attendees");

            migrationBuilder.DropTable(
                name: "NonBankingAttaches",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "ShareHolder_Attaches",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ShareValue");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Association");

            migrationBuilder.DropTable(
                name: "BankingGroups",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "Board");

            migrationBuilder.DropTable(
                name: "BoardMembers",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "Attendees");

            migrationBuilder.DropTable(
                name: "Hospitals");

            migrationBuilder.DropTable(
                name: "NonBankingAuthorizations",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "Shareholders",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "BoardType");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "BoardSettings",
                schema: "Company");

            migrationBuilder.DropTable(
                name: "Companies",
                schema: "Company");
        }
    }
}
