# DocumentsLib

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 13.2.4.

# tailwind

I found a solution for my own problem. One needs to create a static stylesheet file since it is not generated automatically.

    Create the tailwindcss.config.js in the root of your library
    From the root of the library run npx tailwindcss-cli@latest build -o ./src/lib/tailwind.scss
    Include the tailwind.scss file in your component: styleUrls: ['../tailwind.scss']. (Careful with the path)

One still needs to run the npx tailwindcss-cli@latest build -o ./src/lib/tailwind.scss everytime a new class is added to a component to be included into tailwind.scss.

## Install and Run

1. Run `npm install --save <path-to-local-lib>` (example: "/Users/user1/Desktop/documents-lib/dist/documents-lib") in parent angular app.
2. Add in angular.json -> architect -> build -> options.
   {
   ...,
   "preserveSymlinks": true,
   }
3. Add in architect -> build -> options -> assets.
   [
   ...,
   {
   "glob": "**/*",
   "input": "./node_modules/documents-lib/src/assets",
   "output": "/assets/"
   }
   ]
4. Add in app.module.ts
   imports: [
   ...,
   DocumentsLibModule.forRoot({
   configuration: { documentServiceUrl: '', authServiceUrl: '' },
   }),
   ]
5. Add in app-routing.module.ts
   routes: [
   ...,
   { path: '', pathMatch: 'full', redirectTo: '/documents/list' },
   {
   path: '',
   component: DocumentsLibComponent,
   },
   ]
6. Run `ng build --watch` in a lib project.
7. Run `ng serve` in parent project

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.
