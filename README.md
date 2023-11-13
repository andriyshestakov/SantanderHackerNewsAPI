# SantanderHackerNews

## How to run:

One of the options to run is to open SantanderHackerNews.sln and run it in Visual Studio

To query API use Request URL

https://localhost:7050/api/hacker-news/beststories?count=4 


## Assumptions:

Assumption is that our number of requests is an order higher than Hackers News order of updates hence we can cache Best Stories
Best Stories ids can be used as the key to check if there was an update to the Best Stories.
I using caching and lazy initialization. Top stories are loaded as they are queried. 
To invalidate the cache, I compare set of Best Stories IDs. 
The only chance to miss an update is to have item updated but remain in the same list of Best Stories. Solution to this is to use either Firebase update notifications support or check and cache updates from https://hacker-news.firebaseio.com/v0/updates.json and compare them to invalidate the cache. I didn’t implement it due to time limitations.

## If time is given following to be addressed/improved:

Update notifications support

Unit Testing 

Error Handling 
