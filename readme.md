# bigsort - the merge-sort algorithm sample implementation

## The challenge

I've been challenged to sort big files (100GB in size and more).
Each of the input files consists of lines '&lt;number&gt;. &lt;string&gt;'.
For example:
```
415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow 
 ```

 I have to sort lines of the file by the string part first, and then by the number (if the string part is the same).

 The example output would be:
 ```
 1. Apple
 415. Apple
 2. Banana is yellow
 32. Cherry is the best
 30432. Something something something
 ```

This program implements a variation of the merge-sort algorithm for sorting big text files.

## The approach
The classic merge-sort algorithm used in low-memory systems to sort big files not fitting in the RAM. Since the requirements are quite relaxed (I have no memory limit but still the files are a much bigger than the base memory) I've slightly changed the approach. So, this program uses kind of bucket sorting to split the lines by the first two characters. As these buckets are not overlapped we could merge-sort them individually and merge them in order to produce the result file.

## The performance
First, the performance greatly depends on the input dictionary. For highly skewed data distribution we could get not enough buckets to utilize all cores of the CPU. On the other hand having too broad distribution we could get a lot of intermediate chunks and have to merge a lot of files.

Basically the aim of this program is to achieve 10GB file sorting in 10 minutes. It does that on my PC:
CPU: i7-2600,
MB: Asus Sabertooth P67
RAM: 16GB
SDD: Samsung Evo 512

## The command line

The following commands are available (launch with --help to read all available options):

### Generate

Generates the input file of required size (and optionally 3 dictionary types of your choice):

```bash
bigsort.exe generate some-file.txt --limit=10gb
```

### Merge sorting

Performs merge sorting of the input file. Produces the output file with '.out' extension.

```bash
bigsort.exe merge-sort some-file.txt
```

### Check
Checks that the lines in the output file are in the sorted order.
```bash
bigsort.exe merge-sort some-file.out
```
