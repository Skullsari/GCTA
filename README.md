# GCTA pour WoW Forever

Un clic sur l’orbe dans WoW publie la fiche du personnage ciblé dans Discord quand GCTA Companion est lancé.

## Installer sur Windows

1. Sur GitHub, clique sur **Code → Download ZIP**, puis décompresse l’archive.
2. Ferme WoW et double-clique sur **Installer GCTA.cmd** dans le dossier extrait. Le script compile le compagnon sur ton PC avec le compilateur .NET Framework de Windows. Git et Visual Studio ne sont pas nécessaires.
3. Choisis le dossier **World of Warcraft** ou **`_forever_`**.
4. Dans **Réglages**, colle l’URL du webhook reçue en privé, clique sur **Ajouter**, puis **Enregistrer**. Tu peux ajouter plusieurs destinations et activer le démarrage avec Windows.
5. Lance WoW Forever, cible un joueur et clique sur l’orbe GCTA. **Maj + glisser** déplace le bouton.

## Mettre à jour

Pour diagnostiquer un envoi, fais un clic droit sur l’orbe Windows et coche **Activer les logs de diagnostic**, puis fais une nouvelle capture. **Ouvrir le journal** affiche le résultat du recadrage et le code HTTP renvoyé par Discord. Les logs sont désactivés à chaque lancement. Décocher l’option arrête l’enregistrement et conserve le journal déjà écrit.

Télécharge le nouveau ZIP, décompresse-le dans un nouveau dossier et relance **Installer GCTA.cmd**. Accepte la mise à jour. Le dossier WoW, les webhooks, les options Windows et la position du bouton sont conservés. L’installateur te demande de quitter le compagnon s’il est déjà ouvert.

## Relancer ou configurer GCTA

Cherche **GCTA Companion** dans le menu Démarrer. Pour modifier les webhooks, fais un clic droit sur l’orbe près de l’horloge et ouvre **Réglages**. Consulte la [fiche de création d’un webhook](Creer-un-webhook-Discord.txt) si tu configures un salon.

## Alertes SmartScreen et antivirus

Pour l’installateur précompilé **GCTA-Setup.exe**, consulte le [guide SmartScreen et antivirus](https://github.com/Skullsari/GCTA/tree/installateur-exe#alertes-smartscreen-et-antivirus). Il indique comment vérifier le fichier téléchargé et quoi faire selon le message de Windows.

Si Defender met un fichier en quarantaine, relève le nom du fichier et de la détection dans **Sécurité Windows → Protection contre les virus et menaces → Historique de protection**, puis transmets-les au mainteneur de GCTA.

## Créer un webhook Discord

Il te faut la permission **Gérer les webhooks** sur le salon ou le serveur.

1. Dans Discord, ouvre **Paramètres du serveur → Intégrations → Webhooks**.
2. Clique sur **Nouveau webhook**, nomme-le **GCTA** et sélectionne le salon qui recevra les captures, par exemple `#gcta-tests`.
3. Clique sur **Copier l’URL du webhook**.
4. Dans **GCTA Companion → Réglages**, colle cette URL, puis clique sur **Ajouter** et **Enregistrer**. Répète l’opération pour chaque salon supplémentaire.
5. Dans WoW Forever, cible un joueur et clique sur l’orbe GCTA. Vérifie que la carte apparaît dans le salon choisi.

**Garde l’URL privée.** Toute personne qui la possède peut publier dans le salon. Si elle est divulguée, supprime le webhook dans Discord, recrée-le et remplace l’ancienne URL dans GCTA.
