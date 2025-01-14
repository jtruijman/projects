import langdetect
import os


def make_profiles(datafolder, n, limit):
    """
    read all files in a datafolder and for each folder makes an n-gram.
    we provide the first limit frequencies.
    ----------
    
    This function loops over every file in a given datafolder and creates n-gram tables for every file.
    After which the function writes these tables in files located in a folder named after the limit and n of the n-grams
    
    -------
    the output is None, the function does save the ngrams
    """

    for file in os.listdir(datafolder):
        listne = file.split("-")
        file_path = datafolder + "/" + file
        
        if not os.path.exists("./models/" + str(n) + "-" + str(limit)):
            os.makedirs("./models/" + str(n) + "-" + str(limit))
                              
        newfile = "./models/" + str(n) + "-" + str(limit) + "/" + listne[0]
    
        if listne[1] == "Latin1":
            s = "latin1"
        else:
            s = "utf-8"
            
        with open(file_path, "r", encoding=s) as conn:
            fulltext = conn.read()
            boek = langdetect.ngram_table(fulltext, n, limit)
            langdetect.write_ngrams(boek, newfile)    

if __name__ == "__main__":
    make_profiles("./datafiles/training", 2, 200)
    make_profiles("./datafiles/training", 3, 200)

