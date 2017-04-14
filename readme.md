# Project Description

Manga to Epub allow you to convert a bunch of images to a single "epub" file, readable on your reader.
It handles most of the image types as well as several archives. You have multiple customization options, such as trimming the images in order to remove white borders.


### Allowed files:
* images: .bmp, .dib, .emf, .gif, .ico, .jfif, .jpe, .jpeg, .jpg, .rle, .tif, .tiff, .wmf
* archives : .cbr, .cbt, .cbz, .rar, .tar, .zip
This software has actually been tested only with the most common archives and images format (zip, rar, tar, bmp, gif, png and jpg / jpeg) but should theoretically be able to handle all of the above formats.


### Files:
* You can select files in a folder in the order you wish with the first button.
* You can select a folder which will be browsed and any off the files selected, sorted using the same sorter than Windows Explorer, using the second button.
* Required in order to be able to generate the ebook.
* Having any file selected here will unlock the "Preview" button.

### Output Folder:
* Select the output folder for the epub file.
* Initialized to your Documents folder, and "My Books" subfolder if you have one (Sony eReader).
* Required in order to be able to generate the ebook.

### Title:
* The title of your book which will appear on your reader, it is NOT the name of the generated file.

### Author:
* Author of your book.

### Output File:
* Name of the generated file.
* If you do not end it with ".epub", it will automatically be added when you click Generate.
* Required in order to be able to generate the ebook.


The options below are also available for tuning in the "Preview" window that opens if you click the "Preview" button.
This window will display an immediate result of these options on the images you selected.
The generation of your ebook will take between 10 seconds and a minute depending on your processor, the number of images selected and the selected options (Trimming is particularly time consuming).

### Grayscale:
* Will turn each image to grayscale.
* It is not really needed since you will barely gain any disk space with this option and that readers convert automatically colored images to gray.

### Trimming:
* If check, Trim. Level and Trim. Method options will appear, allowing you to select what kind of trimming you want for your images.

### Height:
* Height of the generated images, it will depend on your reader and its resolution.

### Left Margin:
* Between 0 and 1.
* 0 to be aligned to the left.
* 1 to be aligned to the right.
* 0.5 for the image to be centered.

### Trim. Level:
* The higher you choose, the more of the image will be trimmed.
* To see the effect of this option, it is recommended to use the "Preview" button.

### Trim. Method:
* Two different methods, "Average" is actually more of an experiment and is a bit "aggressive" when it comes to trimming and you may lose more of the image than you intended.
* Again, you should use the "Preview" function to have an idea of the result.

### Double Page:
* Gives you different means of handling double pages, such as rotating it or cutting it in two and choosing to have the right page first (right to left reading) or the left page first (left to right reading).

### Offset:
* If you choose either Left Page First or Right Page First in the Double Page option, this one will appear.
* A positive value means that the left part of the original image will be larger than the right part.
* A negative value indicates the opposite


Using this software, you will notice that quite a few values are initialized. It matches the needs I have for my reader (Sony's PRS 650) so it may not be what you need for your reader.
It may be an idea for improvement to offer several default configuration depending on your reader.

This was at first a personal project so it probably suits my needs better than those of anybody else but a colleague of mine suggested that other people may be interested in such a thing so here it is.
It is the first time that I ever publish a work on my own so do not hesitate to criticize, especially the documentation and ergonomics parts ;)

This project requires Microsoft .Net Framework 3.5. It should be available through Windows Update if you do not have it.


Credits:
* To handle RAR archives, I am using the DLL from RarLab : http://www.rarlab.com/rar_add.htm
* To handle TAR archives, I used part of the source code from SharpZipLib : http://www.icsharpcode.net/opensource/sharpziplib/
* To handle ZIP archives and the final generation of the epub file, I am using the DLL from DotNetZip : http://dotnetzip.codeplex.com/
