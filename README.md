# CapgeminiAssignment


Dear reader,

Thank you for taking the time to go through this assignment.

I have included a zipped copy of my solution in debug config titled "Jon_CapgeminiAssignment.

The app is a boilerplate .NET Core web app with changes to Index.cshtml, Index.cshtml.cs, a new class in Model folder called Email_OTP_Module, and some tweaks in Program.cs.

A Nuget package OtpSharp.Core was used because it seemed very lightweight and straightforward to use for a one-off assignment.



TASK 2: Assumptions

1. I tried to adhere to the rubric as much as possible, but the part that tripped me up most was the need to use an input stream for user input of OTP.
Since I was using a .NET Core web app, it felt more natural to do a simple POST, so I took some liberties with that.

2. I assumed that no password was required just for this exercise.

3. send_email() implemented as always returning "STATUS_EMAIL_OK" just for this exercise, but some handling was added for other return codes.

4. Since the email isn't actually being sent, I had the email content rendered as a string instead of html to be returned to the home page.

5. It occurred to me that letting the user send a plaintext OTP code might not be the best idea, but due to time constraints I did not secure this vulnerability. 

6. From a quick google, it seems that SHA512 is no longer considered to be very secure, but I went ahead with it just for this exercise.


TASK 3: Testing

Off the top of my head (its 15min from the deadline submission!!!) I would test the parts of the code which I guess to be more CPU-intensive (like secret key generation and hashing) 
with a much higher volume of simulated requests. Depending on performance, things like the secret key length and and hashing type can be made simpler or more complex. 

In any case, I would like to make sure each method is tested from multiple angles with more edge-case inputs (like null strings etc - the validations can definitely be tightened up).



Final comments:

And now I'm finally done!

I made the huge mistake of accepting this assignment midweek, which left me a few hours after a couple of workdays to complete this. Things got a bit hairy but I gave it my best shot!

I do not have work experience with .NET Core (though I have some personal playtime with it), so I thought doing this assignment with Core would be an appropriate demonstration of where I am (and am not).

In any case, it was quite a fun experience to try these things out. It was my first time building an OTP module, so the research part was quite fun as well. I hope that this webapp will pass as an attempt by a developer looking to transition into .NET Core development.

And if there's anything particularly awful that I did not notice, please let me know! 

Cheers!
