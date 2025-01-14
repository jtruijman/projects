import os
import langdetect
import sys

class LangMatcher:
    """
    This class is used to match text to all kinds of languages.
    """
    def __init__(self, ngram_folder):
        """
        The initializer component is used to store a language name and its n-grams and frequencies in a dictionary
        """
        nlimit = ngram_folder.split("/")
        n_limit = nlimit[-1].split("-")
        self.n = int(n_limit[0])
        self.limit = int(n_limit[1])
        self.lang = dict()
        for file in os.listdir(ngram_folder):
            self.lang[file] = langdetect.read_ngrams(ngram_folder + "/" + file)
        
        
    def score(self, text, k_best=1):
        """
        This function compiles a list of language names and their corresponding cosine similarity score. Which can be
        used to determine which language is contained in a file.
        """
        ngram = langdetect.ngram_table(text, self.n, self.limit)
        scores = dict()
        for taal in self.lang.items():
            scores[taal[0]] = langdetect.cosine_similarity(ngram,taal[1])        
        sorted_scores = sorted(scores.items(), key=lambda x:x[1], reverse=True)

        return sorted_scores[:k_best]


    def recognize(self, filename, encoding="utf-8"):    
        """
        This function opens a file and calls the score function on it and returns the name of the highest matching language.
        """
        with open(filename, encoding=encoding) as conn:
            fulltext = conn.read()

        return self.score(fulltext)
       
     
if __name__ == "__main__":
    if len(sys.argv) > 2:
        check = LangMatcher(sys.argv[1])
        for n in range(2, len(sys.argv)):
            result = check.recognize(sys.argv[n])
            name = sys.argv[n].split("/")
            print("for", name[-1]+":", result[0][0], "with score", result[0][1])
    else:
        print("Er gaat iets niet zo lekker, misschien kan je beter iets anders gaan doen.")
    
