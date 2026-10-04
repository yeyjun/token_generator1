using TokenGenerator.Models;

namespace TokenGenerator.Services
{
    public static class DictionaryService
    {
        public static readonly string[] Categories = { "All", "Latin", "Legal", "Spanish" };

        public static readonly IReadOnlyList<DictionaryEntry> All = new List<DictionaryEntry>
        {
            // Latin
            new() { Category = "Latin", Term = "Carpe diem", Pronunciation = "KAR-pay DEE-em", Definition = "Seize the day; make the most of the present moment.", Example = "She quit her job to travel the world — carpe diem, she said." },
            new() { Category = "Latin", Term = "Et cetera (etc.)", Pronunciation = "et SET-er-uh", Definition = "And so on; and other similar things.", Example = "Bring pens, paper, et cetera for the workshop." },
            new() { Category = "Latin", Term = "Ad hoc", Pronunciation = "ad HOK", Definition = "Formed or done for a particular purpose only, as necessary.", Example = "The committee was an ad hoc group formed to review the incident." },
            new() { Category = "Latin", Term = "Vice versa", Pronunciation = "VY-suh VER-suh", Definition = "The other way round; with the main items in the previous statement reversed.", Example = "Dogs chase cats, and vice versa is rarely true." },
            new() { Category = "Latin", Term = "Alma mater", Pronunciation = "AL-muh MAH-ter", Definition = "The school, college, or university that one attended.", Example = "He still donates to his alma mater every year." },
            new() { Category = "Latin", Term = "Bona fide", Pronunciation = "BOH-nuh fy-dee", Definition = "Genuine; real; made in good faith.", Example = "This is a bona fide offer, not a scam." },
            new() { Category = "Latin", Term = "Curriculum vitae (CV)", Pronunciation = "kuh-RIK-yuh-lum VEE-ty", Definition = "A brief written account of one's education and career history.", Example = "Please attach your curriculum vitae to the application." },
            new() { Category = "Latin", Term = "Et al.", Pronunciation = "et AL", Definition = "And others (used after names to shorten a list of authors).", Example = "The paper was written by Smith et al. in 2020." },
            new() { Category = "Latin", Term = "I.e.", Pronunciation = "that is", Definition = "\"id est\" — that is to say; in other words.", Example = "Bring warm clothing, i.e., a jacket and gloves." },
            new() { Category = "Latin", Term = "E.g.", Pronunciation = "for example", Definition = "\"exempli gratia\" — for example.", Example = "Choose a fruit, e.g., an apple or a banana." },
            new() { Category = "Latin", Term = "Modus operandi", Pronunciation = "MOH-dus op-uh-RAN-dee", Definition = "A particular way or method of doing something, especially a habitual one.", Example = "Leaving no trace was the thief's modus operandi." },
            new() { Category = "Latin", Term = "Per se", Pronunciation = "per SAY", Definition = "By or in itself or oneself; intrinsically.", Example = "It's not the cost, per se, that worries me — it's the risk." },
            new() { Category = "Latin", Term = "Quid pro quo", Pronunciation = "kwid proh KWOH", Definition = "A favor or advantage granted in return for something.", Example = "He offered a quid pro quo: help now, get paid later." },
            new() { Category = "Latin", Term = "Status quo", Pronunciation = "STAY-tus KWOH", Definition = "The existing state of affairs.", Example = "The board voted to maintain the status quo." },
            new() { Category = "Latin", Term = "Verbatim", Pronunciation = "ver-BAY-tim", Definition = "In exactly the same words as were used originally.", Example = "She quoted the speech verbatim in her report." },
            new() { Category = "Latin", Term = "Ad infinitum", Pronunciation = "ad in-fih-NY-tum", Definition = "Again and again in the same way; forever.", Example = "He repeated the same excuse ad infinitum." },
            new() { Category = "Latin", Term = "De facto", Pronunciation = "day FAK-toh", Definition = "In fact, whether by right or not.", Example = "English is the de facto language of business worldwide." },
            new() { Category = "Latin", Term = "In situ", Pronunciation = "in SY-too", Definition = "In its original place; in position.", Example = "The archaeologists studied the fossils in situ." },
            new() { Category = "Latin", Term = "Persona non grata", Pronunciation = "per-SOH-nuh non GRAH-tuh", Definition = "A person who is not welcome somewhere.", Example = "After the scandal, he became persona non grata at the club." },
            new() { Category = "Latin", Term = "Cum laude", Pronunciation = "kum LOW-day", Definition = "With honor or distinction (used of an academic degree).", Example = "She graduated cum laude from law school." },

            // Legal
            new() { Category = "Legal", Term = "Habeas corpus", Pronunciation = "HAY-bee-us KOR-pus", Definition = "A writ requiring a person under arrest to be brought before a court to determine if detention is lawful.", Example = "His lawyer filed a petition for habeas corpus to challenge the detention." },
            new() { Category = "Legal", Term = "Pro bono", Pronunciation = "proh BOH-noh", Definition = "Work undertaken without payment, for the public good.", Example = "The firm handled the case pro bono for the family." },
            new() { Category = "Legal", Term = "Amicus curiae", Pronunciation = "uh-MEE-kus KYOOR-ee-eye", Definition = "\"Friend of the court\" — a party not involved in a case who offers information to assist the court.", Example = "Several organizations filed amicus curiae briefs in the appeal." },
            new() { Category = "Legal", Term = "Voir dire", Pronunciation = "vwahr DEER", Definition = "The process of questioning prospective jurors or witnesses to determine suitability.", Example = "During voir dire, the attorney asked jurors about their prior experiences." },
            new() { Category = "Legal", Term = "Mens rea", Pronunciation = "menz RAY-uh", Definition = "The intention or knowledge of wrongdoing that constitutes part of a crime.", Example = "The prosecution had to prove mens rea beyond a reasonable doubt." },
            new() { Category = "Legal", Term = "Actus reus", Pronunciation = "AK-tus RAY-us", Definition = "The action or conduct that is a constituent element of a crime.", Example = "The actus reus in this case was the unauthorized entry into the building." },
            new() { Category = "Legal", Term = "Subpoena", Pronunciation = "suh-PEE-nuh", Definition = "A writ ordering a person to attend court or produce documents.", Example = "She was served a subpoena to testify next week." },
            new() { Category = "Legal", Term = "Affidavit", Pronunciation = "af-ih-DAY-vit", Definition = "A written statement confirmed by oath, for use as evidence in court.", Example = "He signed an affidavit describing what he witnessed." },
            new() { Category = "Legal", Term = "Plaintiff", Pronunciation = "PLAYN-tif", Definition = "A person who brings a case against another in a court of law.", Example = "The plaintiff is seeking damages for breach of contract." },
            new() { Category = "Legal", Term = "Defendant", Pronunciation = "dih-FEN-dunt", Definition = "An individual or entity accused or sued in a court of law.", Example = "The defendant pleaded not guilty to all charges." },
            new() { Category = "Legal", Term = "Tort", Pronunciation = "tort", Definition = "A wrongful act or infringement of a right leading to civil legal liability.", Example = "Negligence is one of the most common types of tort." },
            new() { Category = "Legal", Term = "Injunction", Pronunciation = "in-JUNK-shun", Definition = "A court order restraining a person from an action, or compelling one.", Example = "The judge granted a temporary injunction to stop the sale." },
            new() { Category = "Legal", Term = "Prima facie", Pronunciation = "PRY-muh FAY-shee-ee", Definition = "Based on the first impression; accepted as correct until proven otherwise.", Example = "The evidence established a prima facie case of fraud." },
            new() { Category = "Legal", Term = "Res judicata", Pronunciation = "rays joo-dih-KAH-tuh", Definition = "A matter that has been decided by a court and cannot be pursued further by the same parties.", Example = "The court dismissed the second lawsuit under res judicata." },
            new() { Category = "Legal", Term = "Stare decisis", Pronunciation = "STAH-ray dih-SY-sis", Definition = "The legal principle of determining points in litigation according to precedent.", Example = "Under stare decisis, the court followed the ruling from the earlier case." },
            new() { Category = "Legal", Term = "Certiorari", Pronunciation = "sur-shee-oh-RAIR-eye", Definition = "A writ by which a higher court reviews the decision of a lower court.", Example = "The Supreme Court granted certiorari to hear the appeal." },
            new() { Category = "Legal", Term = "In camera", Pronunciation = "in KAM-er-uh", Definition = "In private, especially in a judge's chambers, with the public excluded.", Example = "The sensitive testimony was heard in camera." },
            new() { Category = "Legal", Term = "Pro se", Pronunciation = "proh SAY", Definition = "Representing oneself in a court case without a lawyer.", Example = "He chose to appear pro se rather than hire an attorney." },
            new() { Category = "Legal", Term = "Caveat emptor", Pronunciation = "KAV-ee-at EMP-tor", Definition = "\"Let the buyer beware\" — the buyer alone is responsible for checking quality before a purchase.", Example = "Since the car was sold as-is, caveat emptor applied." },
            new() { Category = "Legal", Term = "Force majeure", Pronunciation = "fors mah-ZHUR", Definition = "Unforeseeable circumstances that prevent someone from fulfilling a contract.", Example = "The contract was voided under the force majeure clause after the storm." },

            // Spanish
            new() { Category = "Spanish", Term = "Hola", Pronunciation = "OH-lah", Definition = "Hello.", Example = "¡Hola! ¿Cómo estás? — Hello! How are you?" },
            new() { Category = "Spanish", Term = "Gracias", Pronunciation = "GRAH-syahs", Definition = "Thank you.", Example = "Muchas gracias por tu ayuda. — Thank you very much for your help." },
            new() { Category = "Spanish", Term = "Por favor", Pronunciation = "por fah-VOR", Definition = "Please.", Example = "Pásame el agua, por favor. — Pass me the water, please." },
            new() { Category = "Spanish", Term = "Amigo / Amiga", Pronunciation = "ah-MEE-goh / ah-MEE-gah", Definition = "Friend (male / female).", Example = "Ella es mi mejor amiga. — She is my best friend." },
            new() { Category = "Spanish", Term = "Familia", Pronunciation = "fah-MEE-lyah", Definition = "Family.", Example = "Mi familia vive en la ciudad. — My family lives in the city." },
            new() { Category = "Spanish", Term = "Buenos días", Pronunciation = "BWAY-nohs DEE-ahs", Definition = "Good morning.", Example = "Buenos días, ¿dormiste bien? — Good morning, did you sleep well?" },
            new() { Category = "Spanish", Term = "¿Cómo estás?", Pronunciation = "KOH-moh ess-TAHS", Definition = "How are you?", Example = "¿Cómo estás hoy? — How are you today?" },
            new() { Category = "Spanish", Term = "Adiós", Pronunciation = "ah-DYOHS", Definition = "Goodbye.", Example = "Adiós, nos vemos mañana. — Goodbye, see you tomorrow." },
            new() { Category = "Spanish", Term = "Agua", Pronunciation = "AH-gwah", Definition = "Water.", Example = "Necesito un vaso de agua. — I need a glass of water." },
            new() { Category = "Spanish", Term = "Comida", Pronunciation = "koh-MEE-dah", Definition = "Food; a meal.", Example = "La comida está lista. — The food is ready." },
            new() { Category = "Spanish", Term = "Casa", Pronunciation = "KAH-sah", Definition = "House; home.", Example = "Voy a mi casa después del trabajo. — I'm going home after work." },
            new() { Category = "Spanish", Term = "Trabajo", Pronunciation = "trah-BAH-hoh", Definition = "Work; job.", Example = "Tengo mucho trabajo hoy. — I have a lot of work today." },
            new() { Category = "Spanish", Term = "Amor", Pronunciation = "ah-MOR", Definition = "Love.", Example = "El amor de una madre es incondicional. — A mother's love is unconditional." },
            new() { Category = "Spanish", Term = "Feliz", Pronunciation = "feh-LEES", Definition = "Happy.", Example = "Estoy muy feliz hoy. — I am very happy today." },
            new() { Category = "Spanish", Term = "Grande / Pequeño", Pronunciation = "GRAHN-day / peh-KEH-nyoh", Definition = "Big / small.", Example = "La casa es grande pero el jardín es pequeño. — The house is big but the garden is small." },
            new() { Category = "Spanish", Term = "Hoy / Mañana / Ayer", Pronunciation = "oy / mah-NYAH-nah / ah-YEHR", Definition = "Today / tomorrow / yesterday.", Example = "Hoy trabajo, mañana descanso. — Today I work, tomorrow I rest." },
            new() { Category = "Spanish", Term = "Por supuesto", Pronunciation = "por soo-PWES-toh", Definition = "Of course.", Example = "¿Puedes ayudarme? Por supuesto. — Can you help me? Of course." },
            new() { Category = "Spanish", Term = "Lo siento", Pronunciation = "loh SYEN-toh", Definition = "I'm sorry.", Example = "Lo siento, llegué tarde. — I'm sorry, I arrived late." },
            new() { Category = "Spanish", Term = "¿Dónde está...?", Pronunciation = "DOHN-day ess-TAH", Definition = "Where is...?", Example = "¿Dónde está el baño? — Where is the bathroom?" },
            new() { Category = "Spanish", Term = "Buen provecho", Pronunciation = "bwehn proh-VEH-choh", Definition = "Enjoy your meal.", Example = "¡Buen provecho a todos! — Enjoy your meal, everyone!" },
        };
    }
}
