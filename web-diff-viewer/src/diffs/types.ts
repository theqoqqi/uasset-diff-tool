
export enum DiffType {
    Unchanged = 'Unchanged',
    Changed = 'Changed',
    Added = 'Added',
    Removed = 'Removed',
}

export interface ValueChange<T> {
    DiffType: DiffType;
    From?: T;
    To?: T;
}

export interface FlagsChange<T extends string | number> {
    DiffType: DiffType;
    From?: T;
    To?: T;
    Added?: T;
    Removed?: T;
}

export interface PropertyDiff {
    DiffType: DiffType;
    Name: string;
    PropertyFlags: FlagsChange<string>;
    Type: ValueChange<string>;
    StructClass: ValueChange<string>;
    PropertyClass: ValueChange<string>;
    ArrayDim: ValueChange<number | null>;
    InnerProperties: Record<string, PropertyDiff>;
}

export interface FunctionDiff {
    DiffType: DiffType;
    Name: string;
    FunctionFlags: FlagsChange<string>;
    InputProperties: Record<string, PropertyDiff>;
    OutputProperties: Record<string, PropertyDiff>;
}

export interface AssetDiff {
    DiffType: DiffType;
    Name: string;
    Path: ValueChange<string>;
    Properties: Record<string, PropertyDiff>;
    Functions: Record<string, FunctionDiff>;
}

export type DiffJson = Record<string, AssetDiff>;
