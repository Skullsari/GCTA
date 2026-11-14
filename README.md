# GCTA pour WoW Forever

Un clic sur l’orbe dans WoW publie la fiche du personnage ciblé dans Discord quand GCTA Companion est lancé.

## Installer sur Windows

1. Dans les [releases GitHub](https://github.com/Skullsari/GCTA/releases), télécharge **GCTA-Setup.exe**.
2. Ferme WoW et double-clique sur **GCTA-Setup.exe**. L’installateur contient le compagnon et les fichiers de l’addon.
3. Choisis le dossier **World of Warcraft** ou **`_forever_`**.
4. Dans **Réglages**, colle l’URL du webhook reçue en privé, clique sur **Ajouter**, puis **Enregistrer**. Tu peux ajouter plusieurs destinations et activer le démarrage avec Windows.
5. Lance WoW Forever, cible un joueur et clique sur l’orbe GCTA. **Maj + glisser** déplace le bouton.

## Mettre à jour

Pour diagnostiquer un envoi, fais un clic droit sur l’orbe Windows et coche **Activer les logs de diagnostic**, puis fais une nouvelle capture. **Ouvrir le journal** affiche le résultat du recadrage et le code HTTP renvoyé par Discord. Les logs sont désactivés à chaque lancement. Décocher l’option arrête l’enregistrement et conserve le journal déjà écrit.

Télécharge le nouveau **GCTA-Setup.exe**, ferme WoW et lance l’installateur. Accepte la mise à jour. Le dossier WoW, les webhooks, les options Windows et la position du bouton sont conservés. L’installateur te demande de quitter le compagnon s’il est déjà ouvert.

## Relancer ou configurer GCTA

Cherche **GCTA Companion** dans le menu Démarrer. Pour modifier les webhooks, fais un clic droit sur l’orbe près de l’horloge et ouvre **Réglages**. Consulte la [fiche de création d’un webhook](Creer-un-webhook-Discord.txt) si tu configures un salon.

## Alertes SmartScreen et antivirus

### SmartScreen affiche « Windows a protégé votre ordinateur »

Cette alerte peut apparaître parce que l’installateur GCTA est non signé et que sa réputation est encore inconnue. Télécharge **GCTA-Setup.exe** depuis la [préversion officielle v0.1.10-rc.1](https://github.com/Skullsari/GCTA/releases/tag/v0.1.10-rc.1). Ouvre PowerShell dans le dossier du fichier pour vérifier son empreinte.

```powershell
Get-FileHash -Algorithm SHA256 -LiteralPath '.\GCTA-Setup.exe'
```

Pour **v0.1.10-rc.1**, le résultat attendu est `7926D61E8D2CD930D7D04DC5E3E8E15751A1C7D37E033441239CD60E25043FDC`. Cette empreinte vérifie que le fichier correspond à celui publié sur GitHub.

Si l’empreinte correspond et que SmartScreen affiche uniquement un avertissement d’application inconnue, tu peux choisir **Informations complémentaires → Exécuter quand même**, si Windows propose cette option. [Documentation SmartScreen](https://learn.microsoft.com/fr-fr/windows/apps/package-and-deploy/smartscreen-reputation)

### Defender détecte ou met en quarantaine un fichier

Dans **Sécurité Windows → Protection contre les virus et menaces → Historique de protection**, relève le nom de la détection et le nom du fichier, puis transmets-les au mainteneur de GCTA. Garde le fichier en quarantaine tant que la détection reste inexpliquée.

Si le faux positif est confirmé pour l’installateur, tu peux l’exclure temporairement via **Sécurité Windows → Protection contre les virus et menaces → Gérer les paramètres → Ajouter ou supprimer des exclusions → Ajouter une exclusion → Fichier**. Sélectionne uniquement **GCTA-Setup.exe**, garde la protection en temps réel active et retire l’exclusion après l’installation. [Documentation Microsoft sur les exclusions](https://support.microsoft.com/en-us/defender/antivirus-and-antimalware-software-faq)

## Créer un webhook Discord

Il te faut la permission **Gérer les webhooks** sur le salon ou le serveur.

1. Dans Discord, ouvre **Paramètres du serveur → Intégrations → Webhooks**.
2. Clique sur **Nouveau webhook**, nomme-le **GCTA** et sélectionne le salon qui recevra les captures, par exemple `#gcta-tests`.
3. Clique sur **Copier l’URL du webhook**.
4. Dans **GCTA Companion → Réglages**, colle cette URL, puis clique sur **Ajouter** et **Enregistrer**. Répète l’opération pour chaque salon supplémentaire.
5. Dans WoW Forever, cible un joueur et clique sur l’orbe GCTA. Vérifie que la carte apparaît dans le salon choisi.

**Garde l’URL privée.** Toute personne qui la possède peut publier dans le salon. Si elle est divulguée, supprime le webhook dans Discord, recrée-le et remplace l’ancienne URL dans GCTA.
