-- =============================================
-- FUNewsManagement v2 - Database
-- =============================================
CREATE DATABASE FUNewsManagementV2;
GO
USE FUNewsManagementV2;
GO

-- Tài khoản: AccountRole 1 = Admin, 2 = Staff
CREATE TABLE SystemAccount (
    AccountId       INT IDENTITY(1,1) PRIMARY KEY,
    AccountName     NVARCHAR(100) NOT NULL,
    AccountEmail    NVARCHAR(100) NOT NULL UNIQUE,
    AccountPassword NVARCHAR(255) NOT NULL,
    AccountRole     INT NOT NULL
);

CREATE TABLE Category (
    CategoryId          INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName        NVARCHAR(100) NOT NULL,
    CategoryDescription NVARCHAR(500) NULL,
    IsActive            BIT NOT NULL
);

CREATE TABLE Tag (
    TagId   INT IDENTITY(1,1) PRIMARY KEY,
    TagName NVARCHAR(50) NOT NULL UNIQUE,   -- chống trùng tên tag (màn 4.7)
    Note    NVARCHAR(200) NULL
);

CREATE TABLE NewsArticle (
    NewsArticleId INT IDENTITY(1,1) PRIMARY KEY,
    NewsTitle     NVARCHAR(200) NOT NULL,
    Headline      NVARCHAR(300) NULL,
    NewsContent   NVARCHAR(MAX) NOT NULL,
    NewsSource    NVARCHAR(200) NULL,
    ImageUrl      NVARCHAR(300) NULL,
    NewsStatus    BIT NOT NULL,              -- 1 = Active, 0 = Inactive
    ViewCount     INT NOT NULL DEFAULT 0,    -- dùng cho trending
    CategoryId    INT NOT NULL REFERENCES Category(CategoryId),
    CreatedById   INT NOT NULL REFERENCES SystemAccount(AccountId),
    CreatedDate   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedById   INT NULL REFERENCES SystemAccount(AccountId),
    ModifiedDate  DATETIME2 NULL
);

-- Bảng trung gian bài viết <-> tag
CREATE TABLE NewsTag (
    NewsArticleId INT NOT NULL REFERENCES NewsArticle(NewsArticleId),
    TagId         INT NOT NULL REFERENCES Tag(TagId),
    PRIMARY KEY (NewsArticleId, TagId)
);

CREATE TABLE RefreshToken (
    RefreshTokenId INT IDENTITY(1,1) PRIMARY KEY,
    AccountId      INT NOT NULL REFERENCES SystemAccount(AccountId),
    Token          NVARCHAR(200) NOT NULL UNIQUE,
    ExpiresAt      DATETIME2 NOT NULL,
    IsRevoked      BIT NOT NULL,
    CreatedAt      DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE AuditLog (
    AuditLogId INT IDENTITY(1,1) PRIMARY KEY,
    UserEmail  NVARCHAR(100) NOT NULL,
    Action     NVARCHAR(20)  NOT NULL,      -- Create / Update / Delete
    EntityName NVARCHAR(50)  NOT NULL,      -- Category, Tag, ...
    EntityId   NVARCHAR(50)  NULL,
    BeforeJson NVARCHAR(MAX) NULL,
    AfterJson  NVARCHAR(MAX) NULL,
    Timestamp  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
GO

-- ================= DỮ LIỆU MẪU =================
INSERT INTO SystemAccount (AccountName, AccountEmail, AccountPassword, AccountRole) VALUES
(N'Administrator', 'admin@funews.com',  '123456', 1),
(N'Staff One',     'staff1@funews.com', '123456', 2),
(N'Staff Two',     'staff2@funews.com', '123456', 2);

INSERT INTO Category (CategoryName, CategoryDescription, IsActive) VALUES
(N'Technology', N'Tech news and gadgets', 1),
(N'Education',  N'Schools and learning',  1),
(N'Sports',     N'Sports results',        1),
(N'Health',     N'Health and lifestyle',  0);   -- 1 chuyên mục Inactive để test

INSERT INTO Tag (TagName, Note) VALUES
('AI', NULL), ('Mobile', NULL), ('University', NULL),
('Football', NULL), ('Startup', NULL), ('Research', NULL);

INSERT INTO NewsArticle (NewsTitle, Headline, NewsContent, NewsSource, NewsStatus, ViewCount, CategoryId, CreatedById) VALUES
(N'AI tools change how students learn', N'AI in classrooms',
 N'Universities are adopting AI tools to support students. Research shows AI tutors improve learning results.', N'FU News', 1, 120, 2, 2),
(N'New smartphone chips announced', N'Faster mobile devices',
 N'A startup revealed mobile chips with built-in AI processing for smartphones.', N'Tech Daily', 1, 340, 1, 2),
(N'National football team wins final', N'Historic victory',
 N'The football team won the final match after a dramatic penalty shootout.', N'Sports VN', 1, 560, 3, 3),
(N'Startup raises funds for education app', N'EdTech funding',
 N'An education startup raised new funding to expand its learning app to more universities.', N'FU News', 1, 80, 2, 3),
(N'Research lab builds AI robot', N'Robotics research',
 N'A university research lab built an AI robot that can sort waste automatically.', N'Science Hub', 0, 45, 1, 2),
(N'Mobile games top download charts', N'Gaming trends',
 N'Mobile games dominated app store downloads this month.', N'Tech Daily', 1, 210, 1, 3);

INSERT INTO NewsTag (NewsArticleId, TagId) VALUES
(1,1),(1,3),(1,6),(2,1),(2,2),(2,5),(3,4),(4,5),(4,3),(5,1),(5,6),(6,2);
GO