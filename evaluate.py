import match_language
import os


def eval(model_path, test_path):
    """
    This function is used to evaluate if our language detection algorithm works well.
    It reads the name of the language as well as the encoding from the filename after which the algorithm guesses the
    language of the text in the file. The function then checks if the guessed language is the same as the actual
    language as specified in the filename. It then prints the amount of languages guessed correctly and which
    languages were guessed incorrectly.
    """
    afk_dict = dict()
    ngram_dict = dict()

    for d in [["Danish", "da"],["German", "de"],["Greek", "el"],["English", "en"],
              ["Spanish", "es"],["Finnish", "fi"],["French", "fr"],["Italian", "it"],
              ["Dutch", "nl"],["Portuguese", "pt"],["Swedish", "sv"]]:
        afk_dict[d[0]] = d[1]
    
    for d in [[1, "Unigram"],[2, "Bigram"],[3, "Trigram"],[4, "Quadrigram"],
              [5, "Pentagram"],[6,"Hexagram"]]:
        ngram_dict[d[0]] = d[1]
    
    word = test_path.split("-")[-1]
    test = match_language.LangMatcher(model_path)
    
    for k, v in ngram_dict.items():
        if k == test.n:
            grams = v
            break
        else:
            grams = str(test.n) + "-gram"
    n = 0
    w = 0
    
    for file in os.listdir(test_path):
        for k, v in afk_dict.items():
            if v == file.split(".")[-1]:
                taal = k
        results = test.recognize(test_path+"/"+file)
        if results[0][0] != taal:
            print(file, results[0][0], "Error", taal)
            n += 1
        else:
            print(file, results[0][0])
            w += 1
            
    print(grams, "models for", word + "-word sentences:", w, "correct,", n, "incorrect")
            

if __name__ == "__main__":       
    eval("./models/3-200", "./datafiles/test/europarl-90")
    eval("./models/3-200", "./datafiles/test/europarl-30")
    eval("./models/3-200", "./datafiles/test/europarl-10")
    eval("./models/2-200", "./datafiles/test/europarl-90")
    eval("./models/2-200", "./datafiles/test/europarl-30")
    eval("./models/2-200", "./datafiles/test/europarl-10")            
       