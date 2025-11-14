# Project Setup Guide

## Configuration Folder
Create a `config` folder inside your project. Inside that folder, add a file `config.json` with the following contents:

```json
{
    "GithubToken": "your_github_token_here",
    "GitLabToken": "your_gitlab_token_here",
    "GitLabUrl": "https://gitlab.com"
}
```

Use the GitHub values when integrating with GitHub and the GitLab values when integrating with GitLab.

## Using the Library
This is a library project. To start using it, reference this project from another project in your solution.

