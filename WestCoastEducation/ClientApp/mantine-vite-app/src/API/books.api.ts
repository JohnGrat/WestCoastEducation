import axiosApiInstance from "../Helpers/axios.api.instance";
import BookBriefDto from "../Models/book";

// export const createNoteFn = async (note: CreateNoteInput) => {
//     const response = await axiosApiInstance.post<INoteResponse>("notes/", note);
//     return response.data;
//   };
  
//   export const updateNoteFn = async (noteId: string, note: UpdateNoteInput) => {
//     const response = await axiosApiInstance.patch<INoteResponse>(`notes/${noteId}`, note);
//     return response.data;
//   };
  
//   export const deleteNoteFn = async (noteId: string) => {
//     return axiosApiInstance.delete<null>(`notes/${noteId}`);
//   };
  
//   export const getSingleNoteFn = async (noteId: string) => {
//     const response = await axiosApiInstance.get<INoteResponse>(`notes/${noteId}`);
//     return response.data;
//   };


export interface LoaderOptions {
  sort?: string;
  filter?: string;
  page?: number;
  pageSize?: number;
}
  
export const getBooks = async (options : LoaderOptions) => {
  const response = await axiosApiInstance.get<BookBriefDto[]>(`/book`, {
    params: options,
  });
  return response.data;
};