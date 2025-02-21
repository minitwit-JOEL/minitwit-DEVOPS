let currentUser: any = null;

export const setCurrentUser = (user: any) => {
  currentUser = user;
};

export const getCurrentUser = () => {
  return currentUser;
};

export const clearCurrentUser = () => {
  currentUser = null;
};
