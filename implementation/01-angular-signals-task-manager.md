# Prompt Claude Code — Projet 1 : Angular Signals Task Manager

## Contexte développeur

Je suis ingénieur fullstack avec 7 ans d'expérience Angular / .NET.
Je maîtrise Angular 19+, TypeScript, RxJS, ASP.NET Core, Clean Architecture, TDD, Azure DevOps, Docker.
Je veux apprendre et mettre en pratique Angular Signals dans un projet propre, publiable sur GitHub.

## Objectif du projet

Créer une application de gestion de tâches (Task Manager) en Angular 19+, construite entièrement avec Signals.
Le projet doit démontrer une maîtrise concrète des nouvelles APIs réactives Angular et servir de référence publiable.

## Contraintes techniques obligatoires

- Angular 19+ uniquement — pas de NgModules, tout en Standalone Components
- Réactivité 100% basée sur signal(), computed(), effect() — pas de BehaviorSubject ni RxJS sauf pour les appels HTTP
- State management custom avec Signals — pas de NgRx
- Architecture propre, lisible, maintenable
- Tests unitaires avec Jest ou Vitest (au moins les services et les computed signals)
- Zéro dépendance payante — uniquement des packages open source
- README complet avec instructions d'installation et de lancement
- Pipeline GitHub Actions : build + tests au push

## Fonctionnalités attendues

- Afficher une liste de tâches
- Ajouter / modifier / supprimer une tâche
- Filtrer par statut (toutes / actives / complétées)
- Compteur réactif (nombre de tâches restantes) via computed()
- Persistance locale (localStorage ou in-memory selon ta recommandation)
- UI sobre et fonctionnelle (Angular Material ou Tailwind, ton choix)

## Ce que j'attends de toi

1. Propose une architecture de projet claire (structure des dossiers, découpage des composants et services)
2. Génère le code complet, fichier par fichier
3. Explique chaque choix d'implémentation lié aux Signals (pourquoi signal() ici, pourquoi computed() là)
4. Identifie les pièges courants avec Signals et comment les éviter
5. Génère le fichier GitHub Actions pour CI (build + tests)
6. Génère un README.md professionnel avec badges CI

## Critère de succès

Le projet doit pouvoir être cloné, installé avec `npm install`, lancé avec `ng serve`, et les tests doivent passer avec `ng test`.
Un recruteur technique qui visite le repo doit comprendre immédiatement la valeur du projet.
