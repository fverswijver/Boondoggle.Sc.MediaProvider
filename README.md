# Boondoggle Custom MediaProvider
With support from [![Provided by Boondoggle](http://res.cloudinary.com/dr8gt19s9/image/upload/v1473586972/bd_logo_ej5jjd.gif)](http://www.boondoggle.eu)

This allows you to define custom encoding parameters for media items.

```
<encodeMediaReplacements>
    <replace mode="on" find="&amp;" replaceWith=",-a-," />
    <replace mode="on" find="?" replaceWith=",-q-,"/>
    <replace mode="on" find="/" replaceWith=",-s-,"/>
    <replace mode="on" find="*" replaceWith=",-w-,"/>
    <replace mode="on" find="." replaceWith=",-d-,"/>
    <replace mode="on" find=":" replaceWith=",-c-,"/>
    <replace mode="on" find=" " replaceWith="%20"/>
</encodeMediaReplacements>
```

Source: [http://blog.verswijver.com/custom-mediaprovider-removing-the-dashes/](http://blog.verswijver.com/custom-mediaprovider-removing-the-dashes/)

## Information
* Nuget is used for any dependencies to Sitecore
* As minimal outside dependencies are used (no ORM mapper for example)
* Enjoy!