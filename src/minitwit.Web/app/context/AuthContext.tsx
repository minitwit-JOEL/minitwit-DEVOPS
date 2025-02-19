import React, { createContext, ReactNode, useContext } from "react";
import { UserDto } from "~/types/UserDto";

interface AuthContextType {
  isAuthenticated: boolean;
  user: UserDto | null;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{
  children: ReactNode;
  user: UserDto | null;
}> = ({ children, user }) => {
  return (
    <AuthContext.Provider value={{ isAuthenticated: !!user, user }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
