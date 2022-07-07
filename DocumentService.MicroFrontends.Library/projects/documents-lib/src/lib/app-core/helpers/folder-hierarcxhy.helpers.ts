import { FolderHierarchy } from '../models';

// check if hierarchy loaded
export function checkIfPathExistsInFolderHierarchy(
  tree: FolderHierarchy[],
  path: string
): any {
  return tree.find((element) => {
    if (element.path_url === path && element.childFolder) {
      return true;
    }
    if (element.childFolder && element.childFolder.length > 0) {
      return checkIfPathExistsInFolderHierarchy(element.childFolder, path);
    } else {
      return false;
    }
  })
    ? true
    : false;
}

// expand all on path
export function checkIfTreeExpanedOnPath(
  tree: FolderHierarchy[],
  path: string
): any {
  const newTree = JSON.parse(JSON.stringify(tree));
  return newTree.map((element: any) => {
    if (path != '/' && path.includes(element.path_url)) {
      element.isExpanded = true;
    } else {
      element.isExpanded =
        element.isExpanded != undefined ? element.isExpanded : false;
    }
    if (element.childFolder && element.childFolder.length > 0) {
      element.childFolder = checkIfTreeExpanedOnPath(element.childFolder, path);
    }
    return { ...element };
  });
}

// expand on click ---- only for UI
export function expandFolderOnClick(tree: FolderHierarchy[], path: string) {
  const newTree = JSON.parse(JSON.stringify(tree));
  return newTree.map((element: any) => {
    if (path === element.path_url) {
      element.isExpanded = !element.isExpanded;
    }
    if (element.childFolder && element.childFolder.length > 0) {
      element.childFolder = expandFolderOnClick(element.childFolder, path);
    }
    return { ...element };
  });
}

// find children on path
export function findChildrenFolderOnPath(
  tree: FolderHierarchy[],
  path: string
): any {
  return tree.reduce((a, item) => {
    if (a) return a;
    if (item.path_url === path) return item;
    if (item.childFolder)
      return findChildrenFolderOnPath(item.childFolder, path);
  }, null);
}

// update Tree With Children
export function updateTreeWithChildren(
  tree: FolderHierarchy[],
  childFolder: any,
  path: string
) {
  const newTree = JSON.parse(JSON.stringify(tree));
  return newTree.map((element: any) => {
    if (path === element.path_url) {
      element.childFolder = childFolder;
    }
    if (element.childFolder && element.childFolder.length > 0) {
      element.childFolder = updateTreeWithChildren(
        element.childFolder,
        childFolder,
        path
      );
    }
    return { ...element };
  });
}
