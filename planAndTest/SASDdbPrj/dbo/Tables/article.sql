﻿CREATE TABLE [dbo].[article] (
    [articleId]            UNIQUEIDENTIFIER CONSTRAINT [DF_article_articleId] DEFAULT (newid()) NOT NULL,
    [createtime]           DATETIME2 (7)    CONSTRAINT [DF_article_createtime] DEFAULT (getdate()) NOT NULL,
    [articleTitle]         NVARCHAR (999)   NULL,
    [articleHtmlContent]   NVARCHAR (MAX)   NULL,
    [articleContent]       NVARCHAR (MAX)   NULL,
    [isDir]                BIT              CONSTRAINT [DF_article_isDir] DEFAULT ((0)) NOT NULL,
    [belongToArticleDirId] UNIQUEIDENTIFIER NULL,
    [projectId]            UNIQUEIDENTIFIER NULL,
    [articleType]          VARCHAR (33)     CONSTRAINT [DF_article_articleType] DEFAULT ('GENERAL') NOT NULL,
    [articleStatus]        VARCHAR (33)     CONSTRAINT [DF_article_articleStatus] DEFAULT ('NEW') NOT NULL,
    [priority]             SMALLINT         CONSTRAINT [DF_article_priority] DEFAULT ((9)) NOT NULL,
    [deleteTime]           DATETIME2 (7)    NULL,
    [deleteBy]             VARCHAR (33)     NULL,
    [assignToUserId]       VARCHAR (33)     NULL,
    [authorUserId]         VARCHAR (33)     NULL,
    CONSTRAINT [PK_article] PRIMARY KEY CLUSTERED ([articleId] ASC)
);
















GO
CREATE NONCLUSTERED INDEX [IX_article]
    ON [dbo].[article]([belongToArticleDirId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_article_1]
    ON [dbo].[article]([projectId] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'requirement, issue, solution, etc.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'article', @level2type = N'COLUMN', @level2name = N'articleType';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'new,open,reactivated,resolved.closedassigned', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'article', @level2type = N'COLUMN', @level2name = N'articleStatus';

