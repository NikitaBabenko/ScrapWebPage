CREATE TABLE [dbo].[Prices] (
    [Ticker]       VARCHAR (20) NOT NULL,
    [TimeFrame]    VARCHAR (10) NOT NULL,
    [Date]         DATE         NOT NULL,
    [Time]         INT          NULL,
    [Open]         FLOAT (53)   NULL,
    [High]         FLOAT (53)   NULL,
    [Low]          FLOAT (53)   NULL,
    [Close]        FLOAT (53)   NULL,
    [Volume]       FLOAT (53)   NULL,
    [OpenInterest] INT          NULL,
    PRIMARY KEY CLUSTERED ([Ticker] ASC, [TimeFrame] ASC, [Date] ASC)
);

